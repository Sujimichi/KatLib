using System;
using UnityEngine;
using System.Collections.Generic;

//Built against KSP 1.4.3

namespace KatLib
{

    public struct ClickEvent{
        public bool single_click;
        public bool double_click;
        public bool right_click;
        public Rect contianer;
//        public ClickEvent(){
//            
//        }
    }



    //Base class used in GUIs.  Provides a set of helper methods for GUILayout calls. these helpers take lambda statements and wraps
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
        protected void fspace(){
            GUILayout.FlexibleSpace();
        }

        protected void label(string text){
            GUILayout.Label(text);
        }
        protected void label(string text, GUIStyle style){
            GUILayout.Label(text, style);
        }
        protected void label(string text, GUILayoutOption label_width){
            GUILayout.Label(text, label_width);
        }
        protected void label(string text, GUIStyle style, float label_width){
            GUILayout.Label(text, style, width(label_width));
        }
        protected void label(string text, GUIStyle style, GUILayoutOption label_width){
            GUILayout.Label(text, style, label_width);
        }
        protected void label(Texture texture, float label_width, float label_height){
            GUILayout.Label(texture, width(label_width), height(label_height));
        }
        protected void label(Texture texture, GUIStyle style, float label_width, float label_height){
            GUILayout.Label(texture, style, width(label_width), height(label_height));
        }
        protected void label(Texture texture, GUIStyle style){
            GUILayout.Label(texture, style);
        }

        protected void button(string text, ContentNoArgs action){
            if(GUILayout.Button(text)){action();}
        }
        protected void button(string text, GUIStyle style, ContentNoArgs action){
            if(GUILayout.Button(text, style)){action();}
        }
        protected void button(string text, float button_width, ContentNoArgs action){
            if(GUILayout.Button(text, width(button_width))){action();}
        }
        protected void button(string text, GUIStyle style, float button_width, ContentNoArgs action){
            if(GUILayout.Button(text, style, width(button_width))){action();}
        }
        protected void button(string text, GUIStyle style, float button_width, float max_width, ContentNoArgs action){
            if(GUILayout.Button(text, style, GUILayout.Width(button_width), GUILayout.MaxWidth(max_width), GUILayout.ExpandWidth(true) )){action();}

        }
        protected void button(Texture texture, GUIStyle style, float button_width, float button_height, ContentNoArgs action){            
            if(GUILayout.Button(texture, style, width(button_width), height(button_height))){action();}
        }
        protected void button(GUIContent content, GUIStyle style, float button_width, float button_height, ContentNoArgs action){            
            if(GUILayout.Button(content, style, width(button_width), height(button_height))){action();}
        }



        protected string humanize(float val){
            return String.Format("{0:n}", Math.Round(val, 2)).Replace("_", " ");
        }


        protected void gui_state(bool condition, ContentNoArgs content){
            GUI.enabled = condition;
            content();
            GUI.enabled = true;
        }

        //Definition of delegate to be passed into the section, v_section and scroll methods
        protected delegate void Content(float width);
        protected delegate void ContentNoArgs();
        protected delegate void RectEvents(Rect rect);
        protected delegate void ClickEvents(ClickEvent evt);


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
        protected void section(ContentNoArgs content){
            GUILayout.BeginHorizontal(get_section_style()); 
            content();
            GUILayout.EndHorizontal();                  
        }
        protected void section(float section_width, Content content){
            GUILayout.BeginHorizontal(get_section_style(), GUILayout.Width(section_width), GUILayout.MaxWidth(section_width)); 
            content(section_width);
            GUILayout.EndHorizontal();
        }
        protected void section(float section_width, ContentNoArgs content){
            GUILayout.BeginHorizontal(get_section_style(), GUILayout.Width(section_width), GUILayout.MaxWidth(section_width)); 
            content();
            GUILayout.EndHorizontal();
        }
        protected void section(float section_width, float section_height, ContentNoArgs content){
            GUILayout.BeginHorizontal(get_section_style(), GUILayout.Width(section_width), GUILayout.MaxWidth(section_width), 
                GUILayout.Height(section_height), GUILayout.MaxHeight(section_height)); 
            content();
            GUILayout.EndHorizontal();
        }
        protected void section(float section_width, float section_height, Content content){
            GUILayout.BeginHorizontal(get_section_style(), GUILayout.Width(section_width), GUILayout.MaxWidth(section_width), 
                GUILayout.Height(section_height), GUILayout.MaxHeight(section_height)); 
            content(section_width);
            GUILayout.EndHorizontal();
        }
        protected void section(float section_width, GUIStyle style, Content content){
            GUILayout.BeginHorizontal(style, GUILayout.Width(section_width), GUILayout.MaxWidth(section_width)); 
            content(section_width);
            GUILayout.EndHorizontal();
        }
        protected void section(GUIStyle style, ContentNoArgs content){
            GUILayout.BeginHorizontal(style);
            content();
            GUILayout.EndHorizontal();
        }
        protected void section(float section_width, float section_height, GUIStyle style, Content content){
            GUILayout.BeginHorizontal(style, GUILayout.Width(section_width), GUILayout.MaxWidth(section_width), 
                GUILayout.Height(section_height), GUILayout.MaxHeight(section_height)); 
            content(section_width);
            GUILayout.EndHorizontal();
        }

        protected DateTime click_tracker = DateTime.Now;
        protected Rect last_click_target;
        protected void section(float section_width, GUIStyle style, Content content, ClickEvents click_event){
            GUILayout.BeginHorizontal(style, GUILayout.Width(section_width), GUILayout.MaxWidth(section_width)); 
            content(section_width);
            GUILayout.EndHorizontal();
            Rect container = GUILayoutUtility.GetLastRect();
            handle_click_event(container, click_event);
        }
        protected Rect section(GUIStyle style, ContentNoArgs content, ClickEvents click_event){
            GUILayout.BeginHorizontal(style); 
            content();
            GUILayout.EndHorizontal();
            Rect container = GUILayoutUtility.GetLastRect();
            handle_click_event(container, click_event);
            return container;
        }


        protected void v_section(float section_width, Content content, ClickEvents click_event){
            GUILayout.BeginVertical(get_section_style(), GUILayout.Width(section_width), GUILayout.MaxWidth(section_width)); 
            content(section_width);
            GUILayout.EndVertical();
            Rect container = GUILayoutUtility.GetLastRect();
            handle_click_event(container, click_event);
        }
        protected void v_section(float section_width, float section_height, GUIStyle style, Content content, ClickEvents click_event){
            GUILayout.BeginVertical(style, GUILayout.Width(section_width), GUILayout.MaxWidth(section_width), GUILayout.Height(section_height), GUILayout.MaxHeight(section_height)); 
            content(section_width);
            GUILayout.EndVertical();
            Rect container = GUILayoutUtility.GetLastRect();
            handle_click_event(container, click_event);
        }


        protected void handle_click_event(Rect container, ClickEvents click_event){
            if(container.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown){ 
                ClickEvent evt = new ClickEvent();
                evt.contianer = container;
                if(Event.current.button == 0){
                    double elapsed_seconds = (DateTime.Now - click_tracker).TotalSeconds;
                    click_tracker = DateTime.Now;
                    
                    if(elapsed_seconds < 0.5 && last_click_target == container){
                        evt.double_click = true;
                    } else{
                        evt.single_click = true;
                    }
                } else if(Event.current.button == 1){
                    evt.right_click = true;
                }
                last_click_target = container;
                click_event(evt);
                Event.current.Use();
            }
            
        }


        //Works in the just the same way as section() but wraps the lambda in Begin/End Vertical instead.
        protected void v_section(Content content){
            GUILayout.BeginVertical(get_section_style()); 
            content(win_width_without_padding());
            GUILayout.EndVertical();
        }
        protected void v_section(ContentNoArgs content){
            GUILayout.BeginVertical(get_section_style()); 
            content();
            GUILayout.EndVertical();
        }
        protected void v_section(float section_width, Content content){
            GUILayout.BeginVertical(get_section_style(), GUILayout.Width(section_width), GUILayout.MaxWidth(section_width));
            content(section_width);
            GUILayout.EndVertical();
        }
        protected void v_section(float section_width, GUIStyle style, Content content){
            GUILayout.BeginVertical(style, GUILayout.Width(section_width), GUILayout.MaxWidth(section_width));
            content(section_width);
            GUILayout.EndVertical();
        }
        protected void v_section(GUIStyle style, ContentNoArgs content){
            GUILayout.BeginVertical(style);
            content();
            GUILayout.EndVertical();
        }
        protected void v_section(float section_width, float section_height, Content content){
            GUILayout.BeginVertical(get_section_style(), GUILayout.Width(section_width), GUILayout.MaxWidth(section_width), 
                GUILayout.Height(section_height), GUILayout.MaxHeight(section_height)
            );
            content(section_width);
            GUILayout.EndVertical();
        }
        protected void v_section(float section_width, float section_height, GUIStyle style, Content content){
            GUILayout.BeginVertical(style, GUILayout.Width(section_width), GUILayout.MaxWidth(section_width), 
                GUILayout.Height(section_height), GUILayout.MaxHeight(section_height)
            );
            content(section_width);
            GUILayout.EndVertical();
        }
        protected void v_section(float section_width, float section_height, bool expand_height, Content content){
            GUILayout.BeginVertical(get_section_style(), GUILayout.Width(section_width), GUILayout.MaxWidth(section_width), 
                GUILayout.Height(section_height), GUILayout.MaxHeight(section_height), GUILayout.ExpandHeight(expand_height)
            );
            content(section_width);
            GUILayout.EndVertical();
        }

        protected void area(float area_width, float area_height, Content content){
            GUILayout.BeginArea(new Rect(0, 0, area_width, area_height));
            content(area_width);
            GUILayout.EndArea();
        }

        //Very similar to section() and v_section(), but requires a Vector2 to track scroll position and two floats for width and height as well as the content lamnbda
        //Essentially just the same as section() it wraps the call to the lamba in BeginScrollView/EndScrollView calls.
        //The Vector2 is also returned so it can be passed back in in the next pass of OnGUI
        protected Vector2 scroll(Vector2 scroll_pos, float scroll_width, float scroll_height, Content content){            
            return scroll(scroll_pos, get_section_style(GUI.skin.scrollView), scroll_width, scroll_height, content);
        }
        protected Vector2 scroll(Vector2 scroll_pos, GUIStyle scroll_style, float scroll_width, float scroll_height, Content content){
            scroll_pos = GUILayout.BeginScrollView(scroll_pos, scroll_style, 
                GUILayout.Width(scroll_width), GUILayout.MaxWidth(scroll_width), GUILayout.Height(scroll_height)
            );
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

        private GUIStyle get_section_style(GUIStyle default_style){
            GUIStyle style = style_override == null ? default_style  : style_override;
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
        protected void begin_group(Rect container, GUIStyle group_style, ContentNoArgs content){
            GUI.BeginGroup(container, group_style);
            content();
            GUI.EndGroup();
        }

        //Uses the ComboBox class to setup a drop down menu.
        protected void combobox(string combo_name, Dictionary<int, string> select_options, 
            int selected_id, float list_width, float list_height, DryUI win, ComboResponse resp
        ){
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



        protected Rect default_offset = new Rect(0, 0, 0, 0);

        protected void dropdown(string label, string menu_ref, DropdownMenuData menu, DryUI parent_window, float btn_width, MenuResponse callback){
            dropdown(label, menu_ref, menu, parent_window, btn_width, "Button", "menu.background", "menu.item", callback);
        }


        protected void dropdown(string label, string menu_ref, DropdownMenuData menu, DryUI parent_window, float btn_width, GUIStyle button_style, MenuResponse callback){
            dropdown(label, menu_ref, menu, parent_window, btn_width, button_style, "menu.background", "menu.item", callback);
        }

        protected void dropdown(string label, string menu_ref, DropdownMenuData menu, DryUI parent_window, Rect offset, float btn_width, MenuResponse callback){
            dropdown(label, menu_ref, menu, parent_window, offset, btn_width, "Button", "menu.background", "menu.item", callback);
        }

        protected void dropdown(string label, string menu_ref, DropdownMenuData menu, DryUI parent_window, float btn_width, GUIStyle button_style, GUIStyle menu_style, GUIStyle menu_item_style, MenuResponse callback){
            dropdown(label, menu_ref, menu, parent_window, default_offset, btn_width, button_style, menu_style, menu_item_style, callback);
        }

        protected void dropdown(string label_value, Texture texture, string menu_ref, DropdownMenuData menu, DryUI parent_window, float btn_width, MenuResponse callback){
            dropdown(label_value, texture, menu_ref, menu, parent_window, default_offset, btn_width, "Button", "menu.background", "menu.item", callback);
        }
        protected void dropdown(string label_value, Texture texture, string menu_ref, DropdownMenuData menu, DryUI parent_window, Rect offset, float btn_width, MenuResponse callback){
            dropdown(label_value, texture, menu_ref, menu, parent_window, offset, btn_width, "Button", "menu.background", "menu.item", callback);
        }


        protected void dropdown(Texture label, string menu_ref, DropdownMenuData menu, DryUI parent_window, float btn_width, GUIStyle button_style, GUIStyle menu_style, GUIStyle menu_item_style, MenuResponse callback){
            if(anchors.ContainsKey(menu_ref)){
                menu.set_attributes(anchors[menu_ref], default_offset, parent_window, btn_width, menu_style, menu_item_style, callback);
            }

            if(GUILayout.Button(label, button_style, width(btn_width), height(btn_width))){
                open_dropdown(menu);
            }
            track_rect(menu_ref, GUILayoutUtility.GetLastRect(), true);            
        }
        protected void dropdown(string label, string menu_ref, DropdownMenuData menu, DryUI parent_window, Rect offset, float btn_width, GUIStyle button_style, GUIStyle menu_style, GUIStyle menu_item_style, MenuResponse callback){
            if(anchors.ContainsKey(menu_ref)){
                menu.set_attributes(anchors[menu_ref], offset, parent_window, btn_width, menu_style, menu_item_style, callback);
            }

            if(GUILayout.Button(label, button_style, width(btn_width))){
                open_dropdown(menu);
            }
            track_rect(menu_ref, GUILayoutUtility.GetLastRect(), true);            
        }

        protected void dropdown(string label_value, Texture texture, string menu_ref, DropdownMenuData menu, DryUI parent_window, Rect offset, float btn_width, GUIStyle button_style, GUIStyle menu_style, GUIStyle menu_item_style, MenuResponse callback){
            if(anchors.ContainsKey(menu_ref)){
                menu.set_attributes(anchors[menu_ref], offset, parent_window, btn_width, menu_style, menu_item_style, callback);
            }

            section("Button", () =>{
                label(label_value, "button.text");
                GUILayout.Label(texture, width(16f), height(16f));
            }, evt =>{
                if(evt.single_click){
                    open_dropdown(menu);
                }
            });
            track_rect(menu_ref, GUILayoutUtility.GetLastRect(), true);            
        }


        protected void open_dropdown(DropdownMenuData menu){
            if(Dropdown.instance == null){
                gameObject.AddOrGetComponent<Dropdown>().open(menu);
            } else{
                Dropdown.instance.close_menu();
            }
        }

        protected void track_rect(string name, Rect rect, bool always_track = false){
            if(rect.x != 0 && rect.y != 0){
                if(!anchors.ContainsKey(name) || always_track){
                    anchors[name] = rect;
                }
            }
        }

    }
}

