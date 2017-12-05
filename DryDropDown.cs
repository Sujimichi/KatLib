﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using KSP.UI.Screens;

namespace KatLib
{
    public delegate void MenuResponse(string selected);
    public delegate List<string> DataSource();


    public class DropdownMenuData
    {
        public DropdownMenuData(){
            
        }
        public DropdownMenuData(Dictionary<string, string> data){
            items = data;
        }
        public DropdownMenuData(List<string> data){
            set_data(data);
        }


        public Dictionary<string, string> items = new Dictionary<string, string>();
        public List<string> selected_items = new List<string>();
        public Dictionary<string, string> special_items = new Dictionary<string, string>();
        public string selected_item = null;
        public DataSource remote_data = null; //remote_data enables the menu to pull it's content at the monent it is opened 

        public void set_data(List<string> data){
            items.Clear();
            foreach(string val in data){
                items.Add(val, val);
            }
        }

        public void set_data(Dictionary<string, string> data){
            items.Clear();
            items = data;
        }

        public void fetch_data(){
            if(remote_data != null){
                set_data(remote_data());
            }
        }

        public List<string> values{ 
            get { 
                return new List<string>(items.Values);
            }
        }

        public bool is_selected(string val){
            bool sel = false;
            if(selected_item != null){
                sel = selected_item == val;
            }
            if(selected_items.Contains(val)){
                sel = true;
            }
            return sel;
        }


    }

    public class Dropdown : DryUIBase
    {

        public static Dropdown instance = null;

        public GUISkin skin = null;
        public Rect container = new Rect(0, 0, 0, 0);
        public Rect anchor_rec;
        public Rect anchor_offset;
        public DryUI parent_window;
        public int gui_depth = 0;

        public DropdownMenuData menu_content;
        public List<string> selected_items;

        public MenuResponse resp;
        public float menu_width = 80;
        public float menu_min_width = 80;
        public GUIStyle style_menu = "menu.background";
        public GUIStyle style_menu_item = "menu.item";
        public GUIStyle item_style;
        public Vector2 scroll_pos = new Vector2();
        public float scroll_height = 350f;
        public float scroll_width;


        public void open(Rect anchor, Rect offset, DryUI window, DropdownMenuData menu_data, float width, 
            GUIStyle menu_style, GUIStyle menu_item_style, MenuResponse callback
        ){
            anchor_rec = anchor;
            anchor_offset = offset;
            parent_window = window;
            skin = parent_window.skin;

            menu_content = (DropdownMenuData)menu_data;
            menu_content.fetch_data();

            style_menu = menu_style;
            style_menu_item = menu_item_style;

            foreach(string val in menu_content.values){
                float w = menu_item_style.CalcSize(new GUIContent(val)).x + 15;
                if(w > menu_width){menu_width = w;}
            }

            container.height = 10;
            Vector2 val_size;

            List<string> vals = new List<string>(menu_content.values);
            foreach(KeyValuePair<string, string> pair in menu_content.special_items){
                vals.Add(pair.Value);
            }

            foreach(string val in vals){
                val_size = menu_item_style.CalcSize(new GUIContent(val));
                float w = val_size.x + 15;
                container.height += val_size.y + 4;
                if(w > menu_width){menu_width = w;}
            }               

            container.width = menu_width+5;
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
                        if(menu_content.special_items.Count > 0){                            
                            foreach(KeyValuePair<string, string> pair in menu_content.special_items){
                                if(GUILayout.Button(pair.Value, style_menu_item.name + ".special")){
                                    resp(pair.Key);
                                    close_menu();
                                }
                            }
                        }

                        foreach(KeyValuePair<string, string> pair in menu_content.items){
                            item_style = style_menu_item;
                            if(menu_content.is_selected(pair.Key)){
                                item_style = item_style.name + ".selected";
                            }
                            if(GUILayout.Button(pair.Value, item_style)){
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

