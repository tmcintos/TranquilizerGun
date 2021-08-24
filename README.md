[![Github All Releases](https://img.shields.io/github/downloads/cerberusServers/TranquilizerGun/total.svg)](https://github.com/cerberusServers/TranquilizerGun/releases) [![Maintenance](https://img.shields.io/badge/Maintained%3F-yes-green.svg)](https://github.com/cerberusServers/TranquilizerGun/graphs/commit-activity) [![GitHub license](https://img.shields.io/github/license/Naereen/StrapDown.js.svg)](https://github.com/cerberusServers/TranquilizerGun/blob/main/LICENSE)
<a href="https://github.com/cerberusServers/TranquilizerGun/releases"><img src="https://img.shields.io/github/v/release/cerberusServers/TranquilizerGun?include_prereleases&label=Release" alt="Releases"></a>
<a href="https://discord.gg/PyUkWTg"><img src="https://img.shields.io/discord/656673194693885975?color=%23aa0000&label=EXILED" alt="Support"></a>

# Tranquilizer Gun 2.0
Remade from the ground, here it's the long awaited Tranquilizer Gun plugin! Allowing you to sleep both players and SCPs (Highly configurable).

### What does it do?
This plugin lets you put people to sleep using a pistol randomly found in LCZ & HCZ, one shot uses all your ammo and you will need to shoot SCPs twice to put them to sleep (Everything here is configurable, even the weapon!).

### Installation
As with any EXILED plugin, you must place the TranquilizerGun 2.0.dll file inside of your "EXILED-PTB\Plugins" folder. (Inside %appdata% on Windows)

And... obviously include [EXILED](https://github.com/galaxy119/EXILED "EXILED").

### Commands
Arguments inside &lt;&gt; are required. [] means it's optional.
| Command | Description | Arguments |
| ------------- | ------------------------------ | -------------------- |
| `tg`   | Plugin's main command, sends info. | **protect/toggle/replaceguns/etc**|
- Toggle: Toggles all of the plugin's functions besides it's commands.
- ReplaceGuns: Replaces all the COM15s on the map with a Tranquilizer.
- Protect: Protection against "T-Guns" and Sleep command. (Good for administrators!)
- Sleep: Force Sleep on someone.
- AddGun: Gives you a Tranquilizer Gun.
- Version: Shows you what version of this plugin you're using.

### Configuration
Exiled 2.0 now has auto-generated config files, alongside documentation! So check out your config file for more information on it!


### Permissions
These are the permissions that should be added to your permissions.yml inside your "EXILED-PTB\Configs" folder. (Inside %appdata% on Windows)
| Permission  | This permission belongs to |
| ------------- | ------------- |
| tgun.tg | `tg` and it's arguments | 
| tgun.armor | `tg protect` | 
| tgun.toggle | `tg toggle` | 
| tgun.replaceguns | `tg replaceguns` |
| tgun.sleep | `tg sleep` |
| tgun.givegun | `tg addgun` |
| tgun.* | `All above` | 

### Planned Changes:
- Add more configurable options. (Open to suggestions)
- ~~Changing the "sleeping" system. (Mostly waiting for new SCP:SL patch to hit for effects like slowing someone, stunning them, blurry their vision and stuff like that!)~~
- ~~SCPs Blacklist. (Configurable too)~~

### F.A.Q.:
- **Using older versions of EXILED make this plugin not work:**
*This will not be fixed, but I still get people asking why this plugin doesn't work when they don't have EXILED up to date, so if this plugin doesn't work, try updating to the recommended EXILED version stated in the downloads tab.*

- **The message above says I don't have any bullets but my gun seems like it's shooting, is this a bug?**
*Sadly, most (if not all) animations can't be disabled since they're client-side, everything you see made in EXILED is server-side only. I still haven't found a way to do it, but once I do, don't worry that I'll immediately upload a fix. For the meantime, don't worry, even if it looks like you're shooting, you're not, **IT DOES NOT MAKE YOU SHOOT MORE THAN YOU SHOULD!** (This can be kinda avoided by changing `ammo_used_per_shot` with a value like 9, using the USP)*

- **How do I download?**
*There's a **releases** tab at the side of this Github page, or... just press [here!](https://github.com/cerberusServers/TranquilizerGun/releases)*

### That'd be all
Thanks for passing by, have a nice day! :)
