namespace DirectorEditor
{
    using System;
    using UnityEngine;

    public class DirectorBehaviourControlEventArgs : EventArgs
    {
        public UnityEngine.Behaviour Behaviour;
        public DirectorEditor.DirectorBehaviourControl Control;

        public DirectorBehaviourControlEventArgs(UnityEngine.Behaviour behaviour, DirectorEditor.DirectorBehaviourControl control)
        {
            this.Behaviour = behaviour;
            this.Control = control;
        }
    }
}

