using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using KSP.UI.Screens;

namespace KatLib
{
    public delegate void MenuResponse(string selected);

    public class Dropdown : DryUIBase
    {

        public static Dropdown instance = null;

        public GUISkin skin = null;
        public Rect container = new Rect(0, 0, 0, 0);
        public Rect anchor_rec;
        public Rect anchor_offset;
        public DryUI parent_window;
        public int gui_depth = 0;

        public Dictionary<string, string> menu_content_dict;
        public List<string> menu_content_list;

        public List<string> menu_values;
        public string mode = "dict";

        public List<string> selected_items;
        public MenuResponse resp;
        public float menu_width = 80;
        public float menu_min_width = 80;
        public GUIStyle style_menu = "menu.background";
        public GUIStyle style_menu_item = "menu.item";
        public Vector2 scroll_pos = new Vector2();
        public float scroll_height = 350f;
        public float scroll_width;


        public void open(Rect anchor, Rect offset, DryUI window, object menu_data, object selected, float width, 
            GUIStyle menu_style, GUIStyle menu_item_style, MenuResponse callback
        ){
            anchor_rec = anchor;
            anchor_offset = offset;
            parent_window = window;
            if(selected is string){
                selected_items = new List<string>{ (string)selected };
            } else{
                selected_items = (List<string>)selected;
            }
            skin = parent_window.skin;
            if(menu_data is Dictionary<string, string>){
                menu_content_dict = (Dictionary<string, string>)menu_data;
                menu_values = new List<string>(menu_content_dict.Values);
                mode = "dict";
            }else if(menu_data is List<string>){
                menu_content_list = (List<string>)menu_data;
                menu_values = menu_content_list;
                mode = "list";
            } else{                
                menu_content_list = new List<string> { "menu not correctly setup" };
                menu_values = menu_content_list;
                mode = "list";
            }
            style_menu = menu_style;
            style_menu_item = menu_item_style;

            foreach(string val in menu_values){
                float w = menu_item_style.CalcSize(new GUIContent(val)).x + 15;
                if(w > menu_width){menu_width = w;}
            }

            container.height = 10;
            Vector2 val_size;
            foreach(string val in menu_values){
                val_size = menu_item_style.CalcSize(new GUIContent(val));
//                val_size = DryUI.skin.GetStyle("menu.item").CalcSize(new GUIContent(val));
                float w = val_size.x + 15;
                container.height += val_size.y + 4;
                if(w > menu_width){menu_width = w;}
            }

            container.width = menu_width+5;
//            container.height = menu_values.Count * (menu_item_style.CalcSize(new GUIContent("jeb")).y + 4)+10;
            scroll_width = menu_width+6;
            if(container.height+5 > scroll_height){
                scroll_width += 22;
                container.width += 22;
            }

            resp = callback;
        }

        void Start(){
            instance = this;
        }

        void onDestroy(){
            Debug.Log("destroy called");
            Dropdown.instance = null;    
            Debug.Log("destroyed");
        }

        public void close_menu(){
            parent_window.unlock_ui();
            GameObject.Destroy(Dropdown.instance);
        }

        void OnGUI(){
            GUI.skin = skin;
            GUI.depth = gui_depth;

            container.x = anchor_rec.x + parent_window.window_pos.x + anchor_rec.width - menu_width;
            container.y = anchor_rec.y + parent_window.window_pos.y + anchor_rec.height - 2;
            container.x += anchor_offset.x;
            container.y += anchor_offset.y;

            if(!container.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0){
                close_menu();
                Event.current.Use();
            } else{
                parent_window.lock_ui();
            }

            begin_group(container, "menu.container", () =>{
                
                scroll_pos = scroll(scroll_pos, "menu.scroll", scroll_width, scroll_height, (scroll_inner_width) => {                    
                    style_override = style_menu;
                    v_section(menu_width, w => {
                        GUILayout.Space(2);
                        if(mode == "dict"){
                            foreach(KeyValuePair<string, string> pair in menu_content_dict){
                                GUIStyle item_style = style_menu_item;
                                if(selected_items != null){                                    
                                    if(selected_items.Contains(pair.Key)){     
                                        item_style = item_style.name + ".selected";
                                    }
                                }
                                if(GUILayout.Button(pair.Value, item_style)){
                                    resp(pair.Key);
                                    close_menu();
                                }
                            }                            
                        }else if(mode == "list"){
                            foreach(string val in menu_content_list){
                                GUIStyle item_style = style_menu_item;
                                if(selected_items != null){
                                    if(selected_items.Contains(val)){
                                        item_style = item_style.name + ".selected";
                                    }
                                }
                                if(GUILayout.Button(val, item_style)){
                                    resp(val);
                                    close_menu();
                                }
                            }                            

                        }
                    });
                    
                });
            });
        }

    }


}

