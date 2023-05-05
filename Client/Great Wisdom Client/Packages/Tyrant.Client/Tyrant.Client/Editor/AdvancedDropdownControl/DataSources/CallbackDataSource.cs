using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class CallbackDataSource : AdvancedDropdownDataSource
{
    Func<AdvancedDropdownItem> m_BuildCallback;

    internal CallbackDataSource(Func<AdvancedDropdownItem> buildCallback)
    {
        m_BuildCallback = buildCallback;
    }

    protected override AdvancedDropdownItem FetchData()
    {
        return m_BuildCallback();
    }
}