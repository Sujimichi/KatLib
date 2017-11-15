using System;
using UnityEngine;

namespace KatLib
{
    //DryDialog is lightweight popup (well, dialog) window that takes a lambda statement to define its content.
    //created by the show_dialog method in the DryUI class. ie:
    //show_dialog((d) => {
    //  <GUILayout content stuff>
    //}
    public delegate void DialogContent(DryUI dialog);
    public class DryDialog : DryUI
    {
        public static DryDialog instance = null;
        public DialogContent content;

        private void Start() {
            DryDialog.instance = this;
            footer = false;
            is_dialog = true;
        }

        protected override void WindowContent(int win_id) {            
            content(this);
        }

        public static void close() {
            if (DryDialog.instance) {
                GameObject.Destroy(DryDialog.instance);
            }
        }
    }
}

