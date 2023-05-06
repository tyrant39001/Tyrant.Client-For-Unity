using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public abstract class AdvancedDropdownDataSource
{
    private static readonly string kSearchHeader = L10n.Tr("Search");

    private AdvancedDropdownItem m_MainTree;
    private AdvancedDropdownItem m_SearchTree;
    private List<int> m_SelectedIDs = new List<int>();

    public AdvancedDropdownItem mainTree { get { return m_MainTree; }}
    public AdvancedDropdownItem searchTree { get { return m_SearchTree; }}
    public List<int> selectedIDs { get { return m_SelectedIDs; }}

    protected AdvancedDropdownItem root { get { return m_MainTree; }}
    protected List<AdvancedDropdownItem> m_SearchableElements;

    private static FieldInfo advancedDropdownItem_elementIndexFieldInfo = typeof(AdvancedDropdownItem).GetField("elementIndex", BindingFlags.NonPublic | BindingFlags.Instance);
    public static int GetAdvancedDropdownItemElementIndex(AdvancedDropdownItem advancedDropdownItem)
    {
        return (int)advancedDropdownItem_elementIndexFieldInfo.GetValue(advancedDropdownItem);
    }
    public static void SetAdvancedDropdownItemElementIndex(AdvancedDropdownItem advancedDropdownItem, int value)
    {
        advancedDropdownItem_elementIndexFieldInfo.SetValue(advancedDropdownItem, value);
    }

    public void ReloadData()
    {
        m_SearchableElements = null;
        m_MainTree = FetchData();
    }

    protected abstract AdvancedDropdownItem FetchData();

    public void RebuildSearch(string search)
    {
        m_SearchTree = Search(search);
    }

    protected bool AddMatchItem(AdvancedDropdownItem e, string name, string[] searchWords, List<AdvancedDropdownItem> matchesStart, List<AdvancedDropdownItem> matchesWithin)
    {
        var didMatchAll = true;
        var didMatchStart = false;

        // See if we match ALL the seaarch words.
        for (var w = 0; w < searchWords.Length; w++)
        {
            var search = searchWords[w];
            if (name.Contains(search))
            {
                // If the start of the item matches the first search word, make a note of that.
                if (w == 0 && name.StartsWith(search))
                    didMatchStart = true;
            }
            else
            {
                // As soon as any word is not matched, we disregard this item.
                didMatchAll = false;
                break;
            }
        }
        // We always need to match all search words.
        // If we ALSO matched the start, this item gets priority.
        if (didMatchAll)
        {
            if (didMatchStart)
                matchesStart.Add(e);
            else
                matchesWithin.Add(e);
        }
        return didMatchAll;
    }

    protected virtual AdvancedDropdownItem Search(string searchString)
    {
        if (m_SearchableElements == null)
        {
            BuildSearchableElements();
        }
        if (string.IsNullOrEmpty(searchString))
            return null;

        // Support multiple search words separated by spaces.
        var searchWords = searchString.ToLower().Split(' ');

        // We keep two lists. Matches that matches the start of an item always get first priority.
        var matchesStart = new List<AdvancedDropdownItem>();
        var matchesWithin = new List<AdvancedDropdownItem>();

        foreach (var e in m_SearchableElements)
        {
            var name = e.name.ToLower().Replace(" ", "");
            AddMatchItem(e, name, searchWords, matchesStart, matchesWithin);
        }

        var searchTree = new AdvancedDropdownItem(kSearchHeader);
        matchesStart.Sort();
        foreach (var element in matchesStart)
        {
            searchTree.AddChild(element);
        }
        matchesWithin.Sort();
        foreach (var element in matchesWithin)
        {
            searchTree.AddChild(element);
        }
        return searchTree;
    }

    void BuildSearchableElements()
    {
        m_SearchableElements = new List<AdvancedDropdownItem>();
        BuildSearchableElements(root);
    }

    void BuildSearchableElements(AdvancedDropdownItem item)
    {
        if (!item.children.Any())
        {
            m_SearchableElements.Add(item);
            return;
        }
        foreach (var child in item.children)
        {
            BuildSearchableElements(child);
        }
    }
}

public static class AdvancedDropdownStateExtensions
{
    private static MethodInfo advancedDropdownState_SetSelectedIndexMethodInfo = typeof(AdvancedDropdownState).GetMethod("SetSelectedIndex", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[2] { typeof(AdvancedDropdownItem), typeof(int) }, null);
    private static MethodInfo advancedDropdownState_GetSelectedIndexMethodInfo = typeof(AdvancedDropdownState).GetMethod("GetSelectedIndex", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[1] { typeof(AdvancedDropdownItem) }, null);
    private static MethodInfo advancedDropdownState_GetSelectedChildMethodInfo = typeof(AdvancedDropdownState).GetMethod("GetSelectedChild", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[1] { typeof(AdvancedDropdownItem) }, null);
    private static MethodInfo advancedDropdownState_GetScrollStateMethodInfo = typeof(AdvancedDropdownState).GetMethod("GetScrollState", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[1] { typeof(AdvancedDropdownItem) }, null);
    private static MethodInfo advancedDropdownState_SetScrollStateMethodInfo = typeof(AdvancedDropdownState).GetMethod("SetScrollState", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[2] { typeof(AdvancedDropdownItem), typeof(Vector2) }, null);
    private static MethodInfo advancedDropdownState_MoveDownSelectionMethodInfo = typeof(AdvancedDropdownState).GetMethod("MoveDownSelection", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[1] { typeof(AdvancedDropdownItem) }, null);
    private static MethodInfo advancedDropdownState_MoveUpSelectionMethodInfo = typeof(AdvancedDropdownState).GetMethod("MoveUpSelection", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[1] { typeof(AdvancedDropdownItem) }, null);

    public static void SetSelectedIndex(this AdvancedDropdownState advancedDropdownState, AdvancedDropdownItem item, int index)
    {
        advancedDropdownState_SetSelectedIndexMethodInfo.Invoke(advancedDropdownState, new object[2] { item, index });
    }

    public static int GetSelectedIndex(this AdvancedDropdownState advancedDropdownState, AdvancedDropdownItem item)
    {
        return (int)advancedDropdownState_GetSelectedIndexMethodInfo.Invoke(advancedDropdownState, new object[1] { item });
    }

    public static AdvancedDropdownItem GetSelectedChild(this AdvancedDropdownState advancedDropdownState, AdvancedDropdownItem item)
    {
        return (AdvancedDropdownItem)advancedDropdownState_GetSelectedChildMethodInfo.Invoke(advancedDropdownState, new object[1] { item });
    }

    public static Vector2 GetScrollState(this AdvancedDropdownState advancedDropdownState, AdvancedDropdownItem item)
    {
        return (Vector2)advancedDropdownState_GetScrollStateMethodInfo.Invoke(advancedDropdownState, new object[1] { item });
    }

    public static void SetScrollState(this AdvancedDropdownState advancedDropdownState, AdvancedDropdownItem item, Vector2 scrollState)
    {
        advancedDropdownState_SetScrollStateMethodInfo.Invoke(advancedDropdownState, new object[2] { item, scrollState });
    }

    public static void MoveDownSelection(this AdvancedDropdownState advancedDropdownState, AdvancedDropdownItem item)
    {
        advancedDropdownState_MoveDownSelectionMethodInfo.Invoke(advancedDropdownState, new object[1] { item });
    }

    public static void MoveUpSelection(this AdvancedDropdownState advancedDropdownState, AdvancedDropdownItem item)
    {
        advancedDropdownState_MoveUpSelectionMethodInfo.Invoke(advancedDropdownState, new object[1] { item });
    }
}