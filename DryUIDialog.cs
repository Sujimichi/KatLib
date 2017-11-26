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
        public bool click_out_closes = false;

        private void Start() {
            DryDialog.instance = this;
            footer = false;
            is_dialog = true;
        }

        protected override void WindowContent(int win_id) {            
            content(this);

//            if(click_out_closes){
//                Vector2 mp = Event.current.mousePosition;
//                if( (mp.y < 0 || mp.y > window_pos.y) && (mp.x < 0 || mp.x > window_pos.x) ){
//                    Debug.Log("outside");
////                    close_dialog();
//                }
//            }
        }

        public static void close() {
            if (DryDialog.instance) {
                GameObject.Destroy(DryDialog.instance);
            }
        }
    }
}

