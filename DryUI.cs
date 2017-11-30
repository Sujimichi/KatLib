using System;
using UnityEngine;
using System.Threading;
using System.Collections;


namespace KatLib
{
    /* DryUI is a base class to be inherited by classes which draw GUI windows. It inherits from DryUIBase which in turn inherits from MonoBehaviour
    It provides common setup required to draw a GUI window, enabling DRY and minimal window classes.
    A class which inherits DryUI needs to override the WindowContent method to define the content of the window
    Basic Usage:
        [KSPAddon(KSPAddon.Startup.MainMenu, false)]
        public class SimpleWindow : DryUI
        {
            protected override void WindowContent(int win_id)
            {
                GUILayout.Label ("some nonsense", GUILayout.Width (60f));
            }
        }

    Attributes like the window title, size/pos, id are set, but can be overridden by defining a Start() method
    Configured Usage:
        [KSPAddon(KSPAddon.Startup.MainMenu, false)]
        public class SimpleWindow : DryUI
        {
            private void Start()
            {
                window_pos = new Rect(100,100,500,200); //defaults to new Rect((Screen.width / 2 - 500f / 2), 200, 500f, 5) if not set
                window_title = "test window";           //defaults to "untitled window" if not set
                window_id = 42;                         //defaults to the next ID in sequence (change last_window_id in the base class to change the sequence start point)
                footer = false                          //defaults to true. if true adds a common set of GUI footer elements defined in FooterContent
                draggable = false;                      //defaults to true. makes the windows draggable, duh. 
            }

            protected override void WindowContent(int win_id)
            {
                GUILayout.Label ("some nonsense", GUILayout.Width (60f));
            }
        }    
    DryUI also provides (from DryUIBase) the handy-dandy fabtastic section, scroll and combobox methods. 
    section and v_section take an optional width and a lambda (delegate) statement and wraps the actions defined in the lambda 
    in calls to BeginHorizontal and EndHorizontal (section) or BeginVertical EndVertical (v_section).  
    This ensures End is always called after a begin, and (I think) makes for clearer and more readable code. see DryUIBase for more detail on those.
    */
    public class DryUI : DryUIBase
    {
        public static GUISkin skin        = null;  //static variable to hold the reference to the custom skin.

        //Window Config variables. Change these in Start() in descendent classes.
        public bool prevent_click_through   = true;     //prevent clicks interacting with elements behind the window
        protected bool require_login        = false;    //set to true if the window requires user to be logged into KerbalX
        public bool draggable               = true;     //sets the window as draggable
        public bool footer                  = true;     //sets if the set footer content should be draw (see FooterContent method)
        public bool visible                 = true;     //sets if the window is visible to start with (see show(), hide(), toggle(), on_show(), on_hide())
        protected bool gui_locked           = false;    //if true will disable interaction with the window (without changing its appearance) (see lock_ui() and unlock_ui())
        protected int window_id             = 0;        //can be set to override automatic ID assignment. If left at 0 it will be auto-assigned
        protected static int last_window_id = 0;        //static track of the last used window ID, new windows will take the next value and increment this.
        public string window_title          = "untitled window";    //shockingly enough, this is the window title
        //public Rect window_pos            = new Rect()            //override in Start() to set window size/pos - default values are defined in DryUIBase
//        public int gui_depth                = 0;

        protected bool interface_locked       = false; //not to be confused with gui_locked. interface_lock is set to true when ControlLocks are set on the KSP interface
        protected bool is_dialog            = false; //set to true in dialog windows.



        //show, hide and toggle - basically just change the value of the bool visible which defines whether or not OnGUI will draw the window.
        //also provides hooks (on_show and on_hide) for decendent classes to trigger actions when showing or hiding.
        public void show(){
            visible = true;
            on_show();
        }
        public void hide(){
            visible = false;
            on_hide();
            StartCoroutine(unlock_delay()); //remove any locks on the editor interface, after a slight delay.
        }
        public void toggle(){
            if(visible){
                hide(); 
            } else{
                show();
            }
        }

        //unlock delay just adds a slight delay between an action and unlocking the editor.
        //in cases where a click on the window also results in closing the window (ie a close button) then the click would also get registered by whatever is behind the window
        //adding this short delay prevents that from happening.
        public IEnumerator unlock_delay(){
            yield return true;  //doesn't seem to matter what this returns
            Thread.Sleep(100);
            if(interface_locked){
                InputLockManager.RemoveControlLock(window_id.ToString());
            }
        }

        //overridable methods for gui classes to define actions which are called on hide and show
        protected virtual void on_hide(){}
        protected virtual void on_show(){}
        protected virtual void on_error(){}

        //lock_iu and unlock_ui result in GUI.enabled being set around the call to draw the contents of the window.
        //lets you disable the whole window (it also results in a change to the GUI.color which makes this change without a visual change).
        public void lock_ui(){
            gui_locked = true;
        }
        public void unlock_ui(){
            gui_locked = false;
        }

        //As windows will have been drawn with GUILayout.ExpandHeight(true) setting the height to a small value will cause the window to readjust its height.
        //Only call after actions which reduce the height of the content, don't call it constantly OnGUI (unless Epilepsy is something you enjoy)
        public void autoheight(){
            window_pos.height = 5;
        }


        //opens a dialog window which is populated by the lambda statement passed to show_dialog ie:
        //show_dialog((d) => {
        //  GUILayout.Label("hello I'm a dialog");
        //})
        //The dialog instance is returned by show_dialog, and it's also passed into the lambda.
        protected DryDialog show_dialog(DialogContent content){
            DryDialog dialog = gameObject.AddOrGetComponent<DryDialog>();
            dialog.content = content;
            return dialog;
        }

        //close instance of dialog if it exists.
        protected void close_dialog(){
            DryDialog.close();      
        }

        //basically just syntax sugar for a call to AddOrGetComponent for specific named windows. (unfortunatly has nothing to do with launching rockets)
//        protected void launch(string type){
//            if(type == "ImageSelector"){
//                gameObject.AddOrGetComponent<KerbalXImageSelector>();
//            } else if(type == "ActionGroupEditor"){
//                gameObject.AddOrGetComponent<KerbalXActionGroupInterface>();
//            }
//        }

        //prevents mouse actions on the GUI window from affecting things behind it.
        protected void prevent_ui_click_through(){
            Vector2 mouse_pos = Input.mousePosition;
            mouse_pos.y = Screen.height - mouse_pos.y;
            if(window_pos.Contains(mouse_pos)){
                if(!interface_locked){
                    InputLockManager.SetControlLock(window_id.ToString());
                    interface_locked = true;
                }
            } else{
                if(interface_locked){
                    InputLockManager.RemoveControlLock(window_id.ToString());
                    interface_locked = false;
                }
            }
        }


        //MonoBehaviour methods

        //called on each frame, handles drawing the window and will assign the next window id if it's not set
        protected virtual void OnGUI(){
            if(window_id == 0){
                window_id = last_window_id + 1;
                last_window_id = last_window_id + 1;
            }

            if(visible){
                GUI.skin = skin;
                window_pos = GUILayout.Window(
                    window_id, window_pos, DrawWindow, window_title,
                    GUILayout.Width(window_pos.width), GUILayout.MaxWidth(window_pos.width), GUILayout.ExpandHeight(true)
                );
                GUI.skin = null;
            }
        }

        //Callback method which is passed to GUILayout.Window in OnGUI.  Calls WindowContent and performs common window actions
        protected virtual void DrawWindow(int window_id){
            if(prevent_click_through){
                prevent_ui_click_through();
            }

            if(gui_locked){
                GUI.enabled = false;
                GUI.color = new Color(1, 1, 1, 2); //This enables the GUI to be locked from input, but without changing it's appearance. 
            }
            WindowContent(window_id);   //oh hey, finally, actually drawing the window content. 
            GUI.enabled = true;
            GUI.color = Color.white;

            //add common footer elements for all windows if footer==true
            if(footer){
                FooterContent(window_id);
            }

            //enable draggable window if draggable == true.
            if(draggable){
                GUI.DragWindow();
            }
        }


        //The main method which defines the content of a window.  This method is provided so as to be overridden in inherited classes
        protected virtual void WindowContent(int window_id){

        }

        //Default Footer for all windows, can be overridden only called if footer==true
        protected virtual void FooterContent(int window_id){
        
        }

        protected virtual void OnDestroy(){
            InputLockManager.RemoveControlLock(window_id.ToString()); //ensure control locks are released when GUI is destroyed
        }
    }
}

