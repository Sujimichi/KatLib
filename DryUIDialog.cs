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
        public int gui_depth = 0;

        private void Start() {
            DryDialog.instance = this;
            footer = false;
            is_dialog = true;
            draggable = false;
        }

        protected override void OnGUI(){
            GUI.skin = skin;
            base.OnGUI();
            GUI.skin = null;
        }

        protected override void WindowContent(int win_id) {            
            GUI.depth = gui_depth;
            content(this);
        }

        public static void close() {
            if (DryDialog.instance) {
                GameObject.Destroy(DryDialog.instance);
            }
        }
    }

    public class ModalDialog : DryDialog
    {

        new public Rect window_pos = new Rect(0, 0, Screen.width, Screen.height);

        public Rect dialog_pos = new Rect(0,0,500,80);
        public Rect title_pos = new Rect();

        protected override void OnGUI(){
            if(window_id == 0){
                window_id = last_window_id + 1;
                last_window_id = last_window_id + 1;
            }

            if(visible){
                GUI.skin = skin;
                window_pos = GUI.ModalWindow(
                    window_id, window_pos, DrawWindow, "", skin.box
                );
                GUI.skin = null;
            }

        }

        protected override void WindowContent(int win_id){
            GUI.depth = gui_depth;
            GUILayout.Space(dialog_pos.y);
            title_pos = dialog_pos;
            title_pos.height = 20;
            section(() =>{
                GUILayout.Space(dialog_pos.x);
                GUILayout.BeginVertical("Window", width(dialog_pos.width), height(80f), GUILayout.ExpandHeight(true));
                GUI.Label(title_pos, window_title, "modal.title");
                content(this);
                GUILayout.EndVertical();
            });
        }

    }

}

