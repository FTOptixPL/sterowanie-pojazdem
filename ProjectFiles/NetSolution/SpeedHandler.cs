#region Using directives
using System;
using System.Threading;
using System.Threading.Tasks;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
using FTOptix.Store;
using FTOptix.Alarm;
using FTOptix.EventLogger;
#endregion

public class SpeedHandler : BaseNetLogic
{
    IUANode motorNode;
    IUAVariable speedVar;
    IUAVariable brakeVar;
    IUAVariable acceleratorVar;
    IUAVariable engineVar;

    double targetSpeed = 0;
    double integral = 0;
    double previousError = 0;

    double Kp = 0.8;
    double Ki = 0.2;
    double Kd = 0.1;

    int updateIntervalMs = 200;
    bool pidRunning = false;
    CancellationTokenSource cts;

    public override void Start()
    {
        motorNode = Owner.GetAlias("Alias1");
        if (motorNode == null)
        {
            Log.Error("Alias 'Alias1' nie został ustawiony!");
            return;
        }

        speedVar = motorNode.GetVariable("Speed");
        brakeVar = motorNode.GetVariable("Brake");
        acceleratorVar = motorNode.GetVariable("Accelerator");
        engineVar = motorNode.GetVariable("Engine");

        engineVar.VariableChange += OnEngineChanged;
        acceleratorVar.VariableChange += OnAcceleratorChanged;
        brakeVar.VariableChange += OnBrakeChanged;

        cts = new CancellationTokenSource();
        pidRunning = true;
        Task.Run(() => RunControlLoop(cts.Token));
    }

    public override void Stop()
    {
        engineVar.VariableChange -= OnEngineChanged;
        acceleratorVar.VariableChange -= OnAcceleratorChanged;
        brakeVar.VariableChange -= OnBrakeChanged;

        pidRunning = false;
        cts.Cancel();
    }

    private void OnEngineChanged(object sender, VariableChangeEventArgs e)
    {
        if (!(bool)e.NewValue)
        {
            targetSpeed = 0;
            integral = 0;
            previousError = 0;
        }
    }

    private void OnAcceleratorChanged(object sender, VariableChangeEventArgs e)
    {
        if ((bool)e.NewValue && (bool)engineVar.Value)
        {
            targetSpeed = 100;
        }
        else
        {
            // Gaz puszczony → wyłącz PID
            integral = 0;
            previousError = 0;
        }
    }

    private void OnBrakeChanged(object sender, VariableChangeEventArgs e)
    {
        if ((bool)e.NewValue)
        {
            targetSpeed = 0;
        }
        else
        {
            // Hamulec puszczony → wyłącz PID
            integral = 0;
            previousError = 0;
        }
    }

    private async Task RunControlLoop(CancellationToken token)
    {

        while (pidRunning && !token.IsCancellationRequested)
        {
            try
            {
                double currentSpeed = (int)speedVar.Value;

                bool engineOn = (bool)engineVar.Value;
                bool accel = (bool)acceleratorVar.Value;
                bool brake = (bool)brakeVar.Value;

                if (engineOn)
                {
                    if (accel || brake)
                    {
                        // PID działa tylko gdy gaz lub hamulec
                        double error = targetSpeed - currentSpeed;
                        integral += error * (updateIntervalMs / 1000.0);
                        double derivative = (error - previousError) / (updateIntervalMs / 1000.0);

                        double output = (Kp * error) + (Ki * integral) + (Kd * derivative);

                        double newSpeed = currentSpeed + output * 0.1;
                        if (newSpeed > 100) newSpeed = 100;
                        if (newSpeed < 0) newSpeed = 0;

                        speedVar.Value = (int)newSpeed;
                        previousError = error;
                    }
                    else
                    {
                        // Inercja – powolne wytracanie prędkości
                        double newSpeed = currentSpeed - 0.5; // 0.5 jednostki/s
                        if (newSpeed < 0) newSpeed = 0;
                        speedVar.Value = (int)newSpeed;
                    }
                }
                else
                {
                    // Silnik OFF – szybkie hamowanie
                    double newSpeed = currentSpeed - 2; // 2 jednostki/s
                    if (newSpeed < 0) newSpeed = 0;
                    speedVar.Value = (int)newSpeed;
                }

                await Task.Delay(updateIntervalMs, token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Log.Error($"[ControlLoop] Błąd: {ex.Message}");
            }
        }
    }
}
