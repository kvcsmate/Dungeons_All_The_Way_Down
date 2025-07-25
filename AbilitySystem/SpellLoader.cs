﻿using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace DungeonsAlltheWayDown.AbilitySystem
{
    public class SpellLoader
    {
        public Dictionary<String, PackedScene> SpellScenes = [];
        public Dictionary<String, PackedScene> SpellEffectScenes = [];

        public string SpellDirectory = "Scenes//Nodes//Spells";

        // static private readonly string Root = ":res/";
        public SpellLoader()
        {
            SceneLoader(SpellDirectory);
            foreach (var item in SpellScenes)
            {
                GD.Print(item.Key);
                GD.Print(item.Value);
            }
        }
        
        private void SceneLoader(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory, "*.tscn").ToArray();
            foreach (string fileName in fileEntries)
            {
                AddScene(fileName);
            }
                

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                SceneLoader(subdirectory);
        }

        private void AddScene(string filename)
        {

            string spellName = Path.GetFileNameWithoutExtension(filename);
            
            if (spellName.EndsWith("Effect"))
                SpellEffectScenes.Add(spellName[..^6], (PackedScene)GD.Load(filename));
            else
                SpellScenes.Add(spellName, (PackedScene)GD.Load(filename));

        }

    }


}
