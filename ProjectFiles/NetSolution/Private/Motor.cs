using System;
using UAManagedCore;

//-------------------------------------------
// WARNING: AUTO-GENERATED CODE, DO NOT EDIT!
//-------------------------------------------

[MapType(NamespaceUri = "Faceplate_Demo", Guid = "69e20cf80f6b191a163275e24b60cce2")]
public class Motor : UAObject
{
#region Children properties
    //-------------------------------------------
    // WARNING: AUTO-GENERATED CODE, DO NOT EDIT!
    //-------------------------------------------
    public int Speed
    {
        get
        {
            return (int)Refs.GetVariable("Speed").Value.Value;
        }
        set
        {
            Refs.GetVariable("Speed").SetValue(value);
        }
    }
    public IUAVariable SpeedVariable
    {
        get
        {
            return (IUAVariable)Refs.GetVariable("Speed");
        }
    }
    public bool Engine
    {
        get
        {
            return (bool)Refs.GetVariable("Engine").Value.Value;
        }
        set
        {
            Refs.GetVariable("Engine").SetValue(value);
        }
    }
    public IUAVariable EngineVariable
    {
        get
        {
            return (IUAVariable)Refs.GetVariable("Engine");
        }
    }
    public bool Accelerator
    {
        get
        {
            return (bool)Refs.GetVariable("Accelerator").Value.Value;
        }
        set
        {
            Refs.GetVariable("Accelerator").SetValue(value);
        }
    }
    public IUAVariable AcceleratorVariable
    {
        get
        {
            return (IUAVariable)Refs.GetVariable("Accelerator");
        }
    }
    public bool Brake
    {
        get
        {
            return (bool)Refs.GetVariable("Brake").Value.Value;
        }
        set
        {
            Refs.GetVariable("Brake").SetValue(value);
        }
    }
    public IUAVariable BrakeVariable
    {
        get
        {
            return (IUAVariable)Refs.GetVariable("Brake");
        }
    }
    public string Title
    {
        get
        {
            return (string)Refs.GetVariable("Title").Value.Value;
        }
        set
        {
            Refs.GetVariable("Title").SetValue(value);
        }
    }
    public IUAVariable TitleVariable
    {
        get
        {
            return (IUAVariable)Refs.GetVariable("Title");
        }
    }
    public string Sound
    {
        get
        {
            return (string)Refs.GetVariable("Sound").Value.Value;
        }
        set
        {
            Refs.GetVariable("Sound").SetValue(value);
        }
    }
    public IUAVariable SoundVariable
    {
        get
        {
            return (IUAVariable)Refs.GetVariable("Sound");
        }
    }
#endregion
}
