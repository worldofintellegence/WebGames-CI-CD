#!/usr/bin/env python3
import json
import os
import sys
from datetime import datetime

def update_catalog():
    branch_name = os.environ.get('BRANCH_NAME', 'unknown-branch')
    config_path = os.environ.get('CONFIG_PATH', 'game-config.json')
    games_json_path = os.environ.get('GAMES_JSON_PATH', 'games.json')

    print(f"[Catalog Updater] Branch: {branch_name}")
    print(f"[Catalog Updater] Config Path: {config_path}")
    print(f"[Catalog Updater] Games JSON Path: {games_json_path}")

    # Load game-config.json if it exists
    game_data = {
        "gameId": branch_name,
        "productName": branch_name.replace('-', ' ').title(),
        "description": "WebGL Mini-Game published via GitHub Actions.",
        "category": "Arcade",
        "tags": ["webgl", "unity", branch_name],
        "branch": branch_name,
        "path": f"{branch_name}/",
        "icon": f"{branch_name}/TemplateData/favicon.ico",
        "adsSupported": True,
        "lastUpdated": datetime.utcnow().strftime("%Y-%m-%d")
    }

    if os.path.exists(config_path):
        try:
            with open(config_path, 'r', encoding='utf-8') as f:
                cfg = json.load(f)
                game_data["gameId"] = cfg.get("gameId", game_data["gameId"])
                game_data["productName"] = cfg.get("productName", game_data["productName"])
                game_data["description"] = cfg.get("description", game_data["description"])
                game_data["category"] = cfg.get("category", game_data["category"])
                game_data["tags"] = cfg.get("tags", game_data["tags"])
                print(f"[Catalog Updater] Loaded config for '{game_data['productName']}'")
        except Exception as e:
            print(f"[Catalog Updater] Warning loading config: {e}")

    # Load existing games.json
    games = []
    if os.path.exists(games_json_path):
        try:
            with open(games_json_path, 'r', encoding='utf-8') as f:
                games = json.load(f)
                if not isinstance(games, list):
                    games = []
        except Exception as e:
            print(f"[Catalog Updater] Warning reading existing games.json: {e}")

    # Update or insert game entry by branch or gameId
    updated = False
    for i, g in enumerate(games):
        if g.get("branch") == branch_name or g.get("gameId") == game_data["gameId"]:
            games[i] = game_data
            updated = True
            break

    if not updated:
        games.append(game_data)

    # Save updated games.json
    os.makedirs(os.path.dirname(games_json_path) or '.', exist_ok=True)
    with open(games_json_path, 'w', encoding='utf-8') as f:
        json.dump(games, f, indent=2)

    print(f"[Catalog Updater] Successfully updated {games_json_path} with total {len(games)} games!")

if __name__ == '__main__':
    update_catalog()
