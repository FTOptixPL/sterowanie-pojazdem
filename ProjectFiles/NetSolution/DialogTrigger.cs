
#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.UI;
using FTOptix.Core;
using FTOptix.NetLogic;
#endregion

public class DialogTrigger : BaseNetLogic
{
    private RemoteVariableSynchronizer variableSynchronizer;

    public override void Start()
    {
        // Jedna instancja synchronizatora na całą logikę
        variableSynchronizer = new RemoteVariableSynchronizer();

        // Subskrypcje dla 3 wejść
        Subscribe("InputVariable1", MyVar1_Changed);
        Subscribe("InputVariable2", MyVar2_Changed);
        Subscribe("InputVariable3", MyVar3_Changed);
    }


    /// Pobiera IUAVariable z LogicObject[logicVarName] (NodeId) i subskrybuje zmiany.
    private void Subscribe(string logicVarName, Action<object, VariableChangeEventArgs> handler)
    {
        var logicVar = LogicObject.GetVariable(logicVarName);
        if (logicVar == null || logicVar.Value == null)
        {
            Log.Warning($"{LogicObject.BrowseName}: {logicVarName} nie jest skonfigurowany (brak NodeId).");
            return;
        }

        IUAVariable varNode = InformationModel.GetVariable(logicVar.Value);
        if (varNode == null)
        {
            Log.Warning($"{LogicObject.BrowseName}: {logicVarName} nie można rozwiązać na IUAVariable.");
            return;
        }

        // Jeśli to zdalny Tag — dodać do synchronizatora
        if (varNode.GetType().FullName?.Contains("Tag") == true)
            variableSynchronizer.Add(varNode);

        // Subskrypcja zmian
        varNode.VariableChange += (sender, e) => handler(sender, e);
    }

    private void MyVar1_Changed(object sender, VariableChangeEventArgs e)
    {
        if (ToBool(e.NewValue))
            OpenDialog("AlarmPopup1");
    }

    private void MyVar2_Changed(object sender, VariableChangeEventArgs e)
    {
        if (ToBool(e.NewValue))
            OpenDialog("AlarmPopup2");
    }

    private void MyVar3_Changed(object sender, VariableChangeEventArgs e)
    {
        if (ToBool(e.NewValue))
            OpenDialog("AlarmPopup3");
    }

    private void OpenDialog(string popupVarName)
    {
        var dialogNodeIdVar = LogicObject.GetVariable(popupVarName);
        if (dialogNodeIdVar == null || dialogNodeIdVar.Value == null)
        {
            Log.Warning($"{popupVarName} nie jest skonfigurowany (brak NodeId).");
            return;
        }

        var dialog = InformationModel.Get<DialogType>(dialogNodeIdVar.Value);
        if (dialog == null)
        {
            Log.Warning($"{popupVarName}: nie można rozwiązać DialogType z NodeId.");
            return;
        }

        try
        {
            UICommands.OpenDialog((Window)Owner, dialog);
        }
        catch (Exception ex)
        {
            Log.Warning($"Nie udało się otworzyć {popupVarName}: {ex.Message}");
        }
    }



    private static bool ToBool(UAValue value)
    {
        if (value == null || value.Value == null) return false;
        try { return Convert.ToBoolean(value.Value); }
        catch { return false; }
    }

    public override void Stop()
    {

    }
}