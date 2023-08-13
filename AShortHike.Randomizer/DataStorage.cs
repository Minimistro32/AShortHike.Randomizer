﻿using AShortHike.Randomizer.Connection;
using AShortHike.Randomizer.Items;
using AShortHike.Randomizer.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AShortHike.Randomizer
{
    public class DataStorage
    {
        private readonly string dataPath = Environment.CurrentDirectory + "\\Modding\\data\\Randomizer\\";
        private readonly string configPath = Environment.CurrentDirectory + "\\Modding\\config\\Randomizer.cfg";

        public DataStorage()
        {
            // Create all directories before loading anything
            Directory.CreateDirectory(Environment.CurrentDirectory + "\\Modding\\data");
            Directory.CreateDirectory(Environment.CurrentDirectory + "\\Modding\\config");

            LoadItemsList();

            string locationsPath = dataPath + "item-locations.json";
            if (File.Exists(locationsPath))
                LoadLocationsList(locationsPath);
            else
                Main.LogError("Failed to load locations list from " + locationsPath);

            string imagePath = dataPath + "ap-item.png";
            if (File.Exists(imagePath))
                LoadItemImage(imagePath);
            else
                Main.LogError("Failed to load ap image from " + imagePath);
        }

        // Locations

        private readonly Dictionary<string, ItemLocation> allLocations = new();

        public ItemLocation GetLocationFromId(string locationId)
        {
            return allLocations.TryGetValue(locationId, out ItemLocation location) ? location : null;
        }

        public IEnumerable<ItemLocation> GetAllLocations()
        {
            return allLocations.Values;
        }

        private void LoadLocationsList(string path)
        {
            string json = File.ReadAllText(path);

            foreach (ItemLocation location in JsonConvert.DeserializeObject<ItemLocation[]>(json))
            {
                allLocations.Add(location.gameId, location);
            }

            Main.Log($"Loaded {allLocations.Count} item locations!");
        }

        // Items

        private readonly Dictionary<string, CollectableItem> allItems = new();

        public CollectableItem GetItemFromName(string itemName, out int amount)
        {
            if (allItems.TryGetValue(itemName, out CollectableItem item))
            {
                if (itemName == "Bait")
                    amount = 5;
                else
                    amount = 1;
                return item;
            }

            if (itemName.EndsWith("Coins"))
            {
                amount = int.Parse(itemName.Substring(0, itemName.IndexOf(' ')));
                return allItems.TryGetValue("Coins", out item) ? item : null;
            }

            amount = 0;
            return null;
        }

        private void LoadItemsList()
        {
            // Holdables
            allItems.Add("Stick", CollectableItem.Load("Stick"));
            allItems.Add("Bucket", CollectableItem.Load("Bucket"));
            allItems.Add("Pickaxe", CollectableItem.Load("Pickaxe"));

            // Fishing
            allItems.Add("Fishing Rod", CollectableItem.Load("FishingRod"));
            allItems.Add("Golden Fishing Rod", CollectableItem.Load("GoldenFishingRod"));
            allItems.Add("Fishing Journal", CollectableItem.Load("FishEncyclopedia"));
            allItems.Add("Bait", CollectableItem.Load("Bait"));

            // Clothing
            allItems.Add("Sunhat", CollectableItem.Load("Sunhat"));
            allItems.Add("Baseball Cap", CollectableItem.Load("KidHat"));
            allItems.Add("Provincial Park Hat", CollectableItem.Load("ParkHat"));
            allItems.Add("Headband", CollectableItem.Load("Headband"));
            allItems.Add("Running Shoes", CollectableItem.Load("RunningShoes"));

            // Shovels & Shells
            allItems.Add("Toy Shovel", CollectableItem.Load("ToyShovel"));
            allItems.Add("Shovel", CollectableItem.Load("Shovel"));
            allItems.Add("Seashell", CollectableItem.Load("Shell"));
            allItems.Add("Shell Necklace", CollectableItem.Load("ShellNecklace"));

            // Feathers
            allItems.Add("Golden Feather", CollectableItem.Load("GoldenFeather"));
            allItems.Add("Silver Feather", CollectableItem.Load("SilverFeather"));

            // Maps
            CollectableItem map = CollectableItem.Load("TreasureMap");
            allItems.Add("A Stormy View Map", map);
            allItems.Add("In Her Shadow Map", map);
            allItems.Add("The King Map", map);
            allItems.Add("The Treasure of Sid Beach Map", map);

            // Others
            allItems.Add("Compass", CollectableItem.Load("Compass"));
            allItems.Add("Medal", CollectableItem.Load("Medal"));
            allItems.Add("Wristwatch", CollectableItem.Load("Watch"));
            allItems.Add("Motorboat Key", CollectableItem.Load("BoatKey"));
            allItems.Add("Camping Permit", CollectableItem.Load("CampingPermit"));
            allItems.Add("Coins", CollectableItem.Load("Coin"));

            foreach (CollectableItem item in allItems.Values)
                item.showPrompt = CollectableItem.PickUpPrompt.Always;

            Main.Log($"Loaded {allItems.Count} items!");
        }

        // AP data

        private Dictionary<long, ArchipelagoLocation> apLocations = new(); // Set whenever connecting to server

        public void StoreApLocations(Dictionary<long, ArchipelagoLocation> apLocations)
        {
            this.apLocations = apLocations ?? new Dictionary<long, ArchipelagoLocation>();
        }

        public ArchipelagoLocation GetApDataAtLocation(string locationId)
        {
            ItemLocation location = GetLocationFromId(locationId);
            if (location == null)
                return null;

            return apLocations.TryGetValue(location.apId, out ArchipelagoLocation apLocation) ? apLocation : null;
        }

        // Images

        private Sprite _apImage;
        public Sprite ApImage => _apImage;

        private void LoadItemImage(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(bytes);
            tex.filterMode = FilterMode.Point;

            var size = new Vector2Int(12, 12);
            var rect = new Rect(0, 0, size.x, size.y);
            var pivot = new Vector2(0.5f, 0.5f);
            int pixelsPerUnit = 100;
            var border = Vector4.zero;
            _apImage = Sprite.Create(tex, rect, pivot, pixelsPerUnit, 0, SpriteMeshType.Tight, border);

            Main.Log("Loaded ap item image!");
        }

        // Config

        public SettingsInfo LoadConfig()
        {
            if (File.Exists(configPath))
            {
                return JsonConvert.DeserializeObject<SettingsInfo>(File.ReadAllText(configPath));
            }
            else
            {
                var settings = new SettingsInfo(null, null, null);
                File.WriteAllText(configPath, JsonConvert.SerializeObject(settings, Formatting.Indented));
                return settings;
            }
        }

        public void SaveConfig(SettingsInfo settings)
        {
            File.WriteAllText(configPath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }
    }
}
