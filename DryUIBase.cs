using System;
using UnityEngine;
//using System.Collections;
using System.Collections.Generic;

namespace KatLib
{
    //Base class used in KX GUIs.  Provides a set of helper methods for GUILayout calls. these helpers take lambda statements and wraps
    //them in calls to GUILayout methods.
    public class DryUIBase : MonoBehaviour
    {
        public Rect window_pos = new Rect((Screen.width / 2 - 500f / 2), 200, 500f, 5);
        protected GUIStyle style_override = null;
        protected GUIStyle section_style = new GUIStyle();

        //anchors are used by the ComboBox. Each anchor is a named reference to a Rect obtained from GetLastRect
        public Dictionary<string, Rect> anchors = new Dictionary<string, Rect>();


        //shorthand for GUILayout.width()
        protected GUILayoutOption width(float w){
            return GUILayout.Width(w);
        }
        //shorthand for GUILayout.height()
        protected GUILayoutOption height(float h){
            return GUILayout.Height(h);
        }


        //Essential for any window which needs to make web requests.  If a window is going to trigger web requests then it needs to call this method on its Start() method
        //The RequestHandler handles sending requests asynchronously (so delays in response time don't lag the interface).  In order to do that it uses Coroutines 
        //which are inherited from MonoBehaviour (and therefore can't be triggered by the static methods in KerbalXAPI).
//        protected void enable_request_handler(){
//            if(RequestHandler.instance == null){
//                KerbalX.log("starting web request handler");
//                RequestHandler request_handler = gameObject.AddOrGetComponent<RequestHandler>();
//                RequestHandler.instance = request_handler;
//            }
//        }


        //Definition of delegate to be passed into the section, v_section and scroll methods
        protected delegate void Content(float width);
        protected delegate void ContentNoArgs();


        /* section essentially wraps the actions of a delegate (lambda) in calls to BeginHorizontal and EndHorizontal
         * Can take an optional width float which if given will be passed to BeginHorizontal as GUILayoutOption params for Width and MaxWidth
         * Takes a lambda statement as the delegate Content which is called inbetween calls to Begin/End Horizontal
         * The lambda will be passed a float which is either the width supplied or is the width of the windown (minus padding and margins)
         * Usage:
            section (400f, w => {
                // GUILayout.Label ("some nonsense", GUILayout.Width (w*0.5f)); //use w to get the inner width of the section, 400f in this case
            }); 
            OR without defining a width
            section (w => {
                // GUILayout.Label ("some nonsense", GUILayout.Width (w*0.5f)); //use w to get the inner width of the section, in this case the window width
            }); 
        * In a slightly crazy approach, you can also define a GUIStyle to pass to BeginHorizontal by setting style_override before calling section, ie:
            style_override = new GUIStyle();
            style_override.padding = new RectOffset (20, 20, 10, 10);
            section (w => {
                // GUILayout.Label ("some nonsense", GUILayout.Width (w*0.5f)); //use w to get the inner width of the section, window_pos.width in this case
            }); 
        */
        protected void section(Content content){
            GUILayout.BeginHorizontal(get_section_style()); 
            content(win_width_without_padding());
            GUILayout.EndHorizontal();                  
        } 
        protected void section(float section_width, Content content){
            GUILayout.BeginHorizontal(get_section_style(), GUILayout.Width(section_width), GUILayout.MaxWidth(section_width)); 
            content(section_width);
            GUILayout.EndHorizontal();
        }


        //Works in the just the same way as section() but wraps the lambda in Begin/End Vertical instead.
        protected void v_section(Content content){
            GUILayout.BeginVertical(get_section_style()); 
            content(win_width_without_padding());
            GUILayout.EndVertical();
        }
        protected void v_section(float section_width, Content content){
            GUILayout.BeginVertical(get_section_style(), GUILayout.Width(section_width), GUILayout.MaxWidth(section_width));
            content(section_width);
            GUILayout.EndVertical();
        }
        protected void v_section(float section_width, float section_height, Content content){
            GUILayout.BeginVertical(get_section_style(), GUILayout.Width(section_width), GUILayout.MaxWidth(section_width), GUILayout.Height(section_height));
            content(section_width);
            GUILayout.EndVertical();
        }

        //Very similar to section() and v_section(), but requires a Vector2 to track scroll position and two floats for width and height as well as the content lamnbda
        //Essentially just the same as section() it wraps the call to the lamba in BeginScrollView/EndScrollView calls.
        //The Vector2 is also returned so it can be passed back in in the next pass of OnGUI
        protected Vector2 scroll(Vector2 scroll_pos, float scroll_width, float scroll_height, Content content){
            scroll_pos = GUILayout.BeginScrollView(scroll_pos, get_section_style(), GUILayout.Width(scroll_width), GUILayout.MaxWidth(scroll_width), GUILayout.Height(scroll_height));
            content(scroll_width);
            GUILayout.EndScrollView();
            return scroll_pos;
        }

        //Helper for above section(), v_section() and scroll() methods.  Returns a GUIStyle, either a default GUIStyle() or if section_override has been
        //set then it returns that.  It also sets section_override back to null 
        private GUIStyle get_section_style(){
            GUIStyle style = style_override == null ? section_style : style_override;
            style_override = null;
            return style;
        }
        //Get the window width minus horizontal padding and border (used in above section(), v_section() and scroll() methods when they're not supplied a width)
        private float win_width_without_padding(){
            return window_pos.width - GUI.skin.window.padding.horizontal - GUI.skin.window.border.horizontal;
        }


        protected void begin_group(Rect container, ContentNoArgs content){
            GUI.BeginGroup(container, get_section_style());
            content();
            GUI.EndGroup();
        }


        //Uses the ComboBox class to setup a drop down menu.
        protected void combobox(string combo_name, Dictionary<int, string> select_options, int selected_id, float list_width, float list_height, DryUI win, ComboResponse resp){
            section(list_width, w =>{
                float h = 22f + select_options.Count * 17;
                if(h > list_height){
                    h = list_height;
                }
                if(GUILayout.Button(select_options[selected_id], GUI.skin.textField, width(w - 20f))){
                    gameObject.AddOrGetComponent<ComboBox>().open(combo_name, select_options, anchors[combo_name], h, win, resp);
                }
                track_rect(combo_name, GUILayoutUtility.GetLastRect());
                if(GUILayout.Button("\\/", width(20f))){
                    gameObject.AddOrGetComponent<ComboBox>().open(combo_name, select_options, anchors[combo_name], h, win, resp);
                }
            });     
        }

        protected void track_rect(string name, Rect rect){
            if(rect.x != 0 && rect.y != 0){
                if(!anchors.ContainsKey(name)){
                    anchors[name] = rect;
                }
            }
        }

    }
}

