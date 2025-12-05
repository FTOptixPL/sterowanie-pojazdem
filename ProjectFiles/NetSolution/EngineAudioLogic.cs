#region Using directives
using System;
using System.IO;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using NAudio.Wave;
using FTOptix.Store;
using FTOptix.Alarm;
using FTOptix.EventLogger;
#endregion

// Klasa do zapętlania odtwarzania
public class LoopStream : WaveStream
{
    private readonly WaveStream sourceStream;

    public LoopStream(WaveStream sourceStream)
    {
        this.sourceStream = sourceStream;
    }

    public override WaveFormat WaveFormat => sourceStream.WaveFormat;
    public override long Length => sourceStream.Length;
    public override long Position
    {
        get => sourceStream.Position;
        set => sourceStream.Position = value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int read = sourceStream.Read(buffer, offset, count);
        if (read == 0)
        {
            sourceStream.Position = 0;
            read = sourceStream.Read(buffer, offset, count);
        }
        return read;
    }
}

public class EngineAudioLogic : BaseNetLogic
{
    private const string AudioFolder = @"C:\FTOptix\Faceplate_Demo\ProjectFiles\Audio";
    private const string StartFile = "engine_start.mp3";
    private const string BrakeFile = "brake.mp3";
    private const string DefaultLoopFile = "engine_loop_1.mp3"; // fallback

    IUANode motorNode;
    IUAVariable engineVar;
    IUAVariable brakeVar;
    IUAVariable speedVar;
    IUAVariable soundVar; // dynamiczna nazwa pliku loop

    WaveOutEvent startOutput, engineOutput, brakeOutput;
    AudioFileReader startReader, engineReader, brakeReader;

    public override void Start()
    {
        motorNode = Owner.GetAlias("Alias1");
        if (motorNode == null)
        {
            Log.Error("[Audio] Alias 'Alias1' nie został ustawiony!");
            return;
        }

        engineVar = motorNode.GetVariable("Engine");
        brakeVar  = motorNode.GetVariable("Brake");
        speedVar  = motorNode.GetVariable("Speed");
        soundVar  = motorNode.GetVariable("Sound");

        if (engineVar == null || brakeVar == null || speedVar == null || soundVar == null)
        {
            Log.Error("[Audio] Brak wymaganych zmiennych: Engine / Brake / Speed / Sound");
            return;
        }

        InitStaticAudio();

        engineVar.VariableChange += OnEngineChanged;
        brakeVar.VariableChange  += OnBrakeChanged;
        speedVar.VariableChange  += OnSpeedChanged;

        Log.Info("[Audio] EngineAudioLogic uruchomiony.");
    }

    public override void Stop()
    {
        engineVar.VariableChange -= OnEngineChanged;
        brakeVar.VariableChange  -= OnBrakeChanged;
        speedVar.VariableChange  -= OnSpeedChanged;

        StopAllSounds();
        DisposeAudio();
    }

    private void InitStaticAudio()
    {
        try
        {
            string startPath = Path.Combine(AudioFolder, StartFile);
            string brakePath = Path.Combine(AudioFolder, BrakeFile);

            if (File.Exists(startPath))
            {
                startReader = new AudioFileReader(startPath);
                startOutput = new WaveOutEvent();
                startOutput.Init(startReader);
            }

            if (File.Exists(brakePath))
            {
                brakeReader = new AudioFileReader(brakePath);
                brakeOutput = new WaveOutEvent();
                brakeOutput.Init(brakeReader);
            }
        }
        catch (Exception ex)
        {
            Log.Error("[Audio] Błąd inicjalizacji statycznych plików: " + ex.Message);
        }
    }

    private void OnEngineChanged(object sender, VariableChangeEventArgs e)
    {
        bool engineOn = (bool)e.NewValue;

        if (engineOn)
        {
            PlayStartSound();
            PlayEngineLoopDynamic(); // dynamiczny plik loop z zapętleniem
        }
        else
        {
            if ((int)speedVar.Value == 0)
            {
                StopEngineLoop();
            }
        }
    }

    private void OnBrakeChanged(object sender, VariableChangeEventArgs e)
    {
        bool brakeOn = (bool)e.NewValue;
        if (brakeOn)
            PlayBrakeSound();
        else
            StopBrakeSound();
    }

    private void OnSpeedChanged(object sender, VariableChangeEventArgs e)
    {
        int speed = (int)e.NewValue;
        float volume = Math.Clamp(speed / 100f, 0f, 1f);

        if (engineOutput != null)
            engineOutput.Volume = volume;

        if (!(bool)engineVar.Value && speed == 0)
            StopEngineLoop();
    }

    private void PlayStartSound()
    {
        if (startOutput != null && startReader != null)
        {
            startReader.Position = 0;
            startOutput.Play();
            Log.Info("[Audio] Odtwarzanie: " + StartFile);
        }
    }

    private void PlayEngineLoopDynamic()
    {
        if (soundVar == null) return;

        string soundFile = soundVar.Value.ToString().Trim().Replace("(String)", "").Trim();
        string soundPath = Path.Combine(AudioFolder, soundFile);

        if (!File.Exists(soundPath))
        {
            Log.Warning("[Audio] Brak pliku: " + soundPath + " → używam domyślnego " + DefaultLoopFile);
            soundPath = Path.Combine(AudioFolder, DefaultLoopFile);
            if (!File.Exists(soundPath))
            {
                Log.Error("[Audio] Brak domyślnego pliku: " + soundPath);
                return;
            }
        }

        try
        {
            engineOutput?.Stop();
            engineReader?.Dispose();

            engineReader = new AudioFileReader(soundPath);
            var loopStream = new LoopStream(engineReader); // zapętlenie

            engineOutput = new WaveOutEvent();
            engineOutput.Init(loopStream);
            engineOutput.Volume = Math.Clamp((int)speedVar.Value / 100f, 0f, 1f);
            engineOutput.Play();

            Log.Info("[Audio] Odtwarzanie w pętli: " + soundFile);
        }
        catch (Exception ex)
        {
            Log.Error("[Audio] Błąd odtwarzania loopa: " + ex.Message);
        }
    }

    private void StopEngineLoop() => engineOutput?.Stop();

    private void PlayBrakeSound()
    {
        if (brakeOutput != null && brakeReader != null)
        {
            brakeReader.Position = 0;
            brakeOutput.Play();
            Log.Info("[Audio] Odtwarzanie: " + BrakeFile);
        }
    }

    private void StopBrakeSound() => brakeOutput?.Stop();

    private void StopAllSounds()
    {
        startOutput?.Stop();
        engineOutput?.Stop();
        brakeOutput?.Stop();
    }

    private void DisposeAudio()
    {
        StopAllSounds();
        startOutput?.Dispose();
        engineOutput?.Dispose();
        brakeOutput?.Dispose();
        startReader?.Dispose();
        engineReader?.Dispose();
        brakeReader?.Dispose();
    }
}
