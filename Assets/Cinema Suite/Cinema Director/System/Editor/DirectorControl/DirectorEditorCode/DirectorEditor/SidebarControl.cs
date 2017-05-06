namespace DirectorEditor
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using UnityEditor;
    using UnityEngine;

    public abstract class SidebarControl : DirectorEditor.DirectorBehaviourControl, IComparable
    {
        public int expandedSize = 2;
        public bool isExpanded;

        [field: CompilerGenerated]
        public event DirectorEditor.SidebarControlHandler DuplicateRequest;

        [field: CompilerGenerated]
        public event DirectorEditor.SidebarControlHandler SelectRequest;

        protected SidebarControl()
        {
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            DirectorEditor.SidebarControl control = obj as DirectorEditor.SidebarControl;
            if (control == null)
            {
                throw new ArgumentException("Comparison object is not of type SidebarControl.");
            }
            int num = 0;
            int num2 = Math.Min(this.Ordinal.Length, control.Ordinal.Length);
            for (int i = 0; i < num2; i++)
            {
                num = this.Ordinal[i] - control.Ordinal[i];
                if (num != 0)
                {
                    return num;
                }
            }
            return (this.Ordinal.Length - control.Ordinal.Length);
        }

        public void RequestDuplicate()
        {
            if (this.DuplicateRequest != null)
            {
                this.DuplicateRequest(this, new DirectorEditor.SidebarControlEventArgs(base.Behaviour, this));
            }
        }

        public void RequestSelect()
        {
            if (this.SelectRequest != null)
            {
                this.SelectRequest(this, new DirectorEditor.SidebarControlEventArgs(base.Behaviour, this));
            }
        }

        public void RequestSelect(DirectorEditor.SidebarControlEventArgs args)
        {
            if (this.SelectRequest != null)
            {
                this.SelectRequest(this, args);
            }
        }

        internal void Select()
        {
            GameObject[] gameObjects = Selection.gameObjects;
            ArrayUtility.Add<GameObject>(ref gameObjects, base.Behaviour.gameObject);
            Selection.objects = gameObjects;
        }

        internal void SetExpandedFromEditorPrefs()
        {
            string isExpandedKey = this.IsExpandedKey;
            if (EditorPrefs.HasKey(isExpandedKey))
            {
                this.isExpanded = EditorPrefs.GetBool(isExpandedKey);
            }
            else
            {
                EditorPrefs.SetBool(isExpandedKey, this.isExpanded);
            }
        }

        internal string IsExpandedKey
        {
            get
            {
                SerializedObject obj2 = new SerializedObject(base.Behaviour);
                typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj2, InspectorMode.Debug, null);
                SerializedProperty property = obj2.FindProperty("m_LocalIdentfierInFile");
                return string.Format("Director.{0}.isExpanded", property.intValue);
            }
        }

        public int[] Ordinal { get; set; }
    }
}

