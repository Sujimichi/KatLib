using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using KSP.UI.Screens;

namespace KatLib
{
    public delegate void MenuResponse(string selected);

    public class Dropdown:DryUIBase
    {

        public static Dropdown instance = null;


        public Rect container = new Rect(0, 0, 0, 0);
        public Rect anchor_rec;
        public Rect anchor_offset;
        public DryUI parent_window;
        public int gui_depth = 0;
        public Dictionary<string, string> menu_content;
        public MenuResponse resp;
        public float menu_width = 80;
        public float menu_min_width = 80;
        public GUIStyle style_menu = "menu.background";
        public GUIStyle style_menu_item = "menu.item";
        public Vector2 scroll_pos = new Vector2();
        public float scroll_height = 350f;
        public float scroll_width;


        public void open(Rect anchor, Rect offset, DryUI window, Dictionary<string, string> list, float width, GUIStyle menu_style, GUIStyle menu_item_style, MenuResponse callback){
            anchor_rec = anchor;
            anchor_offset = offset;
            parent_window = window;
            menu_content = list;
            style_menu = menu_style;
            style_menu_item = menu_item_style;

            foreach(KeyValuePair<string, string> pair in list){
                float w = menu_item_style.CalcSize(new GUIContent(pair.Value)).x + 15;
//                float w = GUI.skin.button.CalcSize(new GUIContent(pair.Value)).x + 15;
                if(w > menu_width){menu_width = w;}
            }

            container.width = menu_width+15;
            container.height = list.Count * (menu_item_style.CalcSize(new GUIContent("jeb")).y + 5)+10;
            scroll_width = menu_width+6;
            if(container.height > scroll_height){
                scroll_width += 22;
                container.width += 10;
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
            GUI.skin = DryUI.skin;
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

            begin_group(container, () =>{
                
                scroll_pos = scroll(scroll_pos, "menu.scroll", scroll_width, scroll_height, (scroll_inner_width) => {                    
                    style_override = style_menu;
                    v_section(menu_width, w => {
                        GUILayout.Space(2);
                        foreach(KeyValuePair<string, string> pair in menu_content){
                            if(GUILayout.Button(pair.Value, style_menu_item)){
                                resp(pair.Key);
                                close_menu();
                            }
                        }
                    });
                    
                });
            });
        }

    }


}

