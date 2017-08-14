// A lot of this is Zapk's code from Gamemode_Prop_Hunt. Thanks, buddy.

if(isFile("Add-Ons/System_ReturnToBlockland/server.cs")) {
    RTB_registerPref("Cycle Maps Automatically","Creep Mod","Creep::UseRounds","bool","GameMode_Super_Creeper",1,0,0,0);
    RTB_registerPref("Disable Shop","Creep Mod","Creep::ShopAllowed","bool","GameMode_Super_Creeper",1,0,0,0);
    RTB_registerPref("Disable Bombs","Creep Mod","Creep::NoBombs","bool","GameMode_Super_Creeper",0,0,0,0);
    RTB_registerPref("CreepKill+ Cost Factor","Creep Mod","Creep::KillUpgradeCostFactor","int 1 99999","GameMode_Super_Creeper",500,0,0,0);
} else {
	$Creep::KillUpgradeCostFactor = 500;
    $Creep::UseRounds = 1;
}

$Creep::Init = 1;

function SC_Reload()
{
	exec("Add-Ons/GameMode_Super_Creeper/main.cs");
	messageAll('', "\c6The \c4Super Creeper \c6code has been reloaded by the host.");
}

//		MAIN GAME MODE CODE
function SC_BuildLevelList()
{
	%pattern = "Add-Ons/SuperCreeper_*/save.bls";
	
	$SuperCreeper::numLevels = 0;
	
	%file = findFirstFile(%pattern);
	while(%file !$= "")
	{
		$SuperCreeper::Level[$SuperCreeper::numLevels] = %file;
		$SuperCreeper::numLevels++;

		%file = findNextFile(%pattern);
	}
}

function SC_DumpLevelList()
{
	echo("");
	if($SuperCreeper::numLevels == 1)
		echo("1 level");
	else
		echo($SuperCreeper::numLevels @ " levels");
	for(%i = 0; %i < $SuperCreeper::numLevels; %i++)
	{
		%displayName = $SuperCreeper::Level[%i];
		%displayName = strReplace(%displayName, "Add-Ons/SuperCreeper_", "");
		%displayName = strReplace(%displayName, "/save.bls", "");
		%displayName = strReplace(%displayName, "_", " ");

		if(%i == $SuperCreeper::CurrentLevel)
			echo(" >" @ %displayName);
		else
			echo("  " @ %displayName);
	}
	echo("");
}

function SC_NextLevel()
{
	$SuperCreeper::CurrentLevel = mFloor($SuperCreeper::CurrentLevel);
	$SuperCreeper::CurrentLevel++;
	$SuperCreeper::CurrentLevel = $SuperCreeper::CurrentLevel % $SuperCreeper::numLevels;

	$SuperCreeper::ResetCount = 0;

	SC_LoadLevel_Phase1($SuperCreeper::Level[$SuperCreeper::CurrentLevel]);
}

function SC_LoadLevel_Phase1(%filename)
{
    $DefaultMinigame = Slayer.Minigames.getHostMinigame(); // hotfix

	%mg = $DefaultMiniGame;
	if(!isObject(%mg))
	{
		error("ERROR: SC_LoadLevel( " @ %filename  @ " ) - default minigame not found");
		return;
	}
	for(%i = 0; %i < %mg.numMembers; %i++)
	{
		%client = %mg.member[%i];
		%player = %client.player;
		if(isObject(%player))
			%player.delete();

		%camera = %client.camera;
		%camera.setFlyMode();
		%camera.mode = "Observer";
		%client.setControlObject(%camera);
	}
	
	//clear all bricks 
    if(isObject(CreepGroup)) {
        CreepGroup.ClearSpawns();
    }
    
    // note: this function is deferred, so we'll have to set a callback to be triggered when it's done
	BrickGroup_888888.chaindeletecallback = "SC_LoadLevel_Phase2(\"" @ %filename @ "\");";
	BrickGroup_888888.chaindeleteall();
}

function SC_LoadLevel_Phase2(%filename)
{
	echo("Loading Super Creeper save " @ %filename);

	%displayName = %filename;
	%displayName = strReplace(%displayName, "Add-Ons/SuperCreeper_", "");
	%displayName = strReplace(%displayName, "/save.bls", "");
	%displayName = strReplace(%displayName, "_", " ");
	
	%loadMsg = "\c6Now loading \c4" @ %displayName;

	//read and display credits file, if it exists
	// limited to one line
	%creditsFilename = filePath(%fileName) @ "/credits.txt";
	if(isFile(%creditsFilename))
	{
		%file = new FileObject();
		%file.openforRead(%creditsFilename);

		%line = %file.readLine();
		%line = stripMLControlChars(%line);
		%loadMsg = %loadMsg @ "\c6, created by \c4" @ %line;

		%file.close();
		%file.delete();
	}

	messageAll('', %loadMsg);
    
    // load super creeper config
	%configFilename = filePath(%fileName) @ "/config.cs";
    exec(%configFilename);
    
    // show map config messages
    
    // Gamemode
    switch($Creep::GameMode) {
        case 0:
            messageAll('', "\c6- The gamemode for this map is \c4Man Vs Creeper\c6. Get your CreepKill ready.");
        case 1:
            messageAll('', "\c6- The gamemode for this map is \c4Creepageddon\c6. You are defenseless. Be the last one alive!");
    }
    
    if($Creep::TimeLimit > 0) {
        messageAll('', "\c6- The time limit for this map is \c4" @ $Creep::TimeLimit @ "\c6 minutes.");
    } else {
        messageAll('', "\c6- There is no time limit for this map.");
        
        if($Creep::GameMode == 1 || $Creep::GameMode == 3 || $Creep::GameMode == 4 || $Creep::GameMode == 5) {
            messageAll('', "WARNING: The current gamemode will not work properly without a time limit. Consider adding one in your map's config.cs.");
        }
    }
    
    $DefaultMiniGame.time = $Creep::TimeLimit; // bit of a hack again
    
    if($Creep::GameMode == 4 || $Creep::GameMode == 5) {
        messageAll('', "\c6- The creeper must grow to \c4" @ $Creep::RequiredAmmount @ "\c6 bricks to win.");
    }
    
    // Creeper
    //messageAll('', "\c6- The Creeper HP factor for this map is\c4" SPC $Creep::HPFactor @ "\c6.");
    //messageAll('', "\c6- The Creeper Speed on this map is\c4" SPC $Creep::tickms SPC "\c6miliseconds per block.");
    //messageAll('', "\c6- The Creeper's Self Awareness level on this map is\c4" SPC $Creep::WanderLust SPC "\c6. Stay back!");
    
    //if($Creep::regen) {
    //    messageAll('', "\c6- Creeper Health Regeneration is \c4On \c6for this map. The speed is\c4" SPC $Creep::regenms SPC "\c6miliseconds.");
    //} else {
    //    messageAll('', "\c6- Creeper Health Regeneration is \c4Off \c6for this map.");
    //}
    
    // commented out because nobody understands or needs to know this stuff
    
    if($Creep::UseShop) {
        messageAll('', "\c6- The shop is \c4On \c6for this map.");
    } else {
        messageAll('', "\c6- The shop is \c4Off \c6for this map.");
    }
    
    // who wins on time up?
    %playerwinontimeup = 0;
    %creeperwinontimeup = 0;
    
    if($Creep::GameMode == 0 || $Creep::GameMode == 1 || $Creep::GameMode == 5) {
        %playerwinontimeup = 1;
    }
    if($Creep::GameMode == 3) {
        %creeperwinontimeup = 1;
    }
    
    $DefaultMiniGame.Teams.getTeamFromName("The Players").winOnTimeUp = %playerwinontimeup;
    $DefaultMiniGame.Teams.getTeamFromName("The Creeper").winOnTimeUp = %creeperwinontimeup;
    
    //Slayer.Prefs.setPref("Victory Method","Lives",%onelife,%mini);
    
	//load environment if it exists
	%envFile = filePath(%fileName) @ "/environment.txt"; 
	if(isFile(%envFile))
	{  
		//echo("parsing env file " @ %envFile);
		//usage: GameModeGuiServer::ParseGameModeFile(%filename, %append);
		//if %append == 0, all minigame variables will be cleared 
		%res = GameModeGuiServer::ParseGameModeFile(%envFile, 1);

		EnvGuiServer::getIdxFromFilenames();
		EnvGuiServer::SetSimpleMode();

		if(!$EnvGuiServer::SimpleMode)	  
		{
			EnvGuiServer::fillAdvancedVarsFromSimple();
			EnvGuiServer::SetAdvancedMode();
		}
	}
	
    //let the gamemode start
    if($Creep::Init == 1) {
        $Creep::Init = 0;
    }
    
	//load save file
	schedule(10, 0, serverDirectSaveFileLoad, %fileName, 3, "", 2, 1);
}


//		COMMANDS FOR THE MANS
function serverCmdLevelList(%client)
{
	for(%i = 0; %i < $SuperCreeper::numLevels; %i++)
	{
		%displayName = $SuperCreeper::Level[%i];
		%displayName = strReplace(%displayName, "Add-Ons/SuperCreeper_", "");
		%displayName = strReplace(%displayName, "/save.bls", "");
		%displayName = strReplace(%displayName, "_", " ");

		if(%i == $SuperCreeper::CurrentLevel)
			messageClient(%client, '', "\c4" @ %i @ "\c6. " @ %displayName SPC "(\c4Selected\c6)");
		else
			messageClient(%client, '', "\c4" @ %i @ "\c6. " @ %displayName);
	}
}

function servercmdReloadLevels(%client)
{
	if(!%client.isSuperAdmin)
	{
		messageClient(%client, '', "\c6You are not super admin!");
		return;
	}
	messageAll('', "\c4" @ %client.name SPC "\c6reloaded the Super Creeper level cache.");
	setModPaths(getModPaths());
	SC_BuildLevelList();
}
function serverCmdSetLevel(%client, %i)
{
	if(!%client.isAdmin)
		return;

	if(mFloor(%i) !$= %i)
	{
		messageClient(%client, '', "Usage: /setLevel <number>");
		return;
	}

	if(%i < 0 || %i > $SuperCreeper::numLevels)
	{
		messageClient(%client, '', "serverCmdSetLevel() - out of range index");
		return;
	}

	messageAll( 'MsgAdminForce', '\c4%1\c6 changed the level', %client.getPlayerName());
	
	$SuperCreeper::CurrentLevel = %i - 1;
	SC_NextLevel();
}
function serverCmdNextLevel(%client, %i)
{
	if(!%client.isAdmin)
		return;

	messageAll( 'MsgAdminForce', '\c4%1\c6 changed the level', %client.getPlayerName());
	
	SC_NextLevel();
}

function serverCmdHelp(%client) {
    MessageClient(%client,'',"\c2-Super Creeper Commands-");
    MessageClient(%client,'',"\c2/help\c3 - shows this message.");
    MessageClient(%client,'',"\c2/rules\c3 - shows the rules.");
    MessageClient(%client,'',"\c2/upgrade\c3 - Allows you to buy items for use in certain gamemodes.");
    MessageClient(%client,'',"\c2/levellist\c3 - Shows all the maps available.");
    
    if(%client.isAdmin || %client.isSuperAdmin) {
        MessageClient(%client,'',"\c2/nextlevel\c3 - go to the next map, skipping all other rounds of this one.");
        MessageClient(%client,'',"\c2/reloadlevels\c3 - loads new levels from Add-Ons without restarting the server.");
        MessageClient(%client,'',"\c2/setlevel\c3 - loads a level. Use a levelID.");
        MessageClient(%client,'',"\c2/forcekill\c3 - get rid of people who are minorly breaking the rules.");
    }
}

function serverCmdRules(%client) {
    MessageClient(%client,'',"\c2-Rules-");
    MessageClient(%client,'',"\c3- Do not spam.");
    MessageClient(%client,'',"\c3- Do not floathack.");
    MessageClient(%client,'',"\c3- Do not deliberately hold up the game.");
    MessageClient(%client,'',"\c3- Do not block other players in non-competitive modes.");
    MessageClient(%client,'',"\c3- Do not annoy others.");
}

function serverCmdUpgrade(%client, %buy) {
    if(UpgradeMan.getValue(%client.bl_id,"score") $= "") {
        return;
    }
    
    if(!$Creep::ShopAllowed) {
        MessageClient(%client,'',"\c2The shop is disabled.");
        return;
    }

    %nextCreepKillCost = UpgradeMan.getValue(%client.bl_id,"creepKillLvl")*$Creep::KillUpgradeCostFactor;
    %nextCreepBombCost = (UpgradeMan.getValue(%client.bl_id,"creepBombLvl")+1)*($Creep::KillUpgradeCostFactor*1.5);
    
    if(UpgradeMan.getValue(%client.bl_id,"creepKillLvl") == 4) {
        %nextCreepKillCost = "MAXED";
    }
    
    if(UpgradeMan.getValue(%client.bl_id,"creepBombLvl") == 2) {
        %nextCreepBombCost = "MAXED";
    }
    
    if(%buy $= "") {
        MessageClient(%client,'',"\c2-UPGRADES-");
        //MessageClient(%client,'',"\c3The shop is work in progress. Don't get too attached to items you buy.");
        MessageClient(%client,'',"\c3Use \c2/upgrade [name] \c3to buy:");
        MessageClient(%client,'',"\c2CreepKill \c3- Upgrade your CreepKill item so you can destroy creeper more quickly. (\c2" @ %nextCreepKillCost @ "\c3)");
        
        if(!$Creep::NoBombs) {
            MessageClient(%client,'',"\c2CreepBomb \c3- Anti-Creeper grenades. (\c2" @ %nextCreepBombCost @ "\c3)");
        }
        
        return;
    }
    
    if(%buy $= "CreepKill") {
        if(%nextCreepKillCost $= "MAXED") {
            MessageClient(%client,'',"\c2Your CreepKill is maxed out!");
            return;
        }
        
        if(UpgradeMan.getValue(%client.bl_id,"score") >= %nextCreepKillCost) {
            UpgradeMan.incValue(%client.bl_id, "score", -%nextCreepKillCost);
            UpgradeMan.incValue(%client.bl_id, "creepKillLvl", 1);
            
            %client.UpdateSuperCreeperScore();
            giveNewCreepKill(%client, UpgradeMan.getValue(%client.bl_id, "creepKillLvl"));
            
            MessageClient(%client,'',"\c2Your CreepKill item has been upgraded!");
        } else {
            MessageClient(%client,'',"\c2You need more points. Try to win some games.");
        }
    }
    
    if(%buy $= "CreepBomb" && !$Creep::NoBombs) {
        if(%nextCreepBombCost $= "MAXED") {
            MessageClient(%client,'',"\c2Your CreepBomb is maxed out!");
            return;
        }
        
        if(UpgradeMan.getValue(%client.bl_id,"score") >= %nextCreepBombCost) {
            UpgradeMan.incValue(%client.bl_id, "score", -%nextCreepBombCost);
            UpgradeMan.incValue(%client.bl_id, "creepBombLvl", 1);
            
            %client.UpdateSuperCreeperScore();
            giveNewCreepBomb(%client, UpgradeMan.incValue(%client.bl_id, "creepBombLvl", 1));
            
            MessageClient(%client,'',"\c2Your CreepBomb item has been upgraded!");
        } else {
            MessageClient(%client,'',"\c2You need more points. Try to win some games.");
        }
    }
}

function giveNewCreepKill(%client,%level) {
    %player = %client.player;
    if(!isObject(%player)) return;
    
    // remove old CreepKill
    switch(%level) {
        case 2:
            %player.removeItem(CreepKillWeakGunItem);
        case 3:
            %player.removeItem(CreepKillGunItem);
        case 4:
            %player.removeItem(CreepKillStrongGunItem);
    }
    
    // give new CreepKill
    switch(%level) {
        case 2:
            %player.addItem(nameToID(CreepKillGunItem),%client);
        case 3:
            %player.addItem(nameToID(CreepKillStrongGunItem),%client);
        case 4:
            %player.addItem(nameToID(CreepKillVStrongGunItem),%client);
    }
}

function giveNewCreepBomb(%client,%level) {
    %player = %client.player;
    if(!isObject(%player)) return;
    
    // remove old CreepBomb (if it's already been used the command will handle it)
    if(%level == 2) {
        %player.removeItem(CreepBombItem);
    }
    
    // give new CreepBomb
    switch(%level) {
        case 1:
            %player.addItem(nameToID(CreepBombItem),%client);
        case 2:
            %player.addItem(nameToID(CreepBomb2Item),%client);
    }
}

function serverCmdForceKill(%cl, %vic) {
    if(%cl.isAdmin || %cl.isSuperAdmin) {
        findclientbyname(%vic).player.kill();
        MessageClient(findclientbyname(%vic),'',"\c0Do not break the rules.");
    }
}

//	PACKAGE
deactivatepackage(GameModeSuperCreeperPackage);
package GameModeSuperCreeperPackage
{
	function fxDTSBrick::setVehicle(%obj, %data, %client)
	{
	  return;
	}
	function fxDTSBrick::setItem(%obj, %data, %client)
	{
	  return;
	}
	//this is called when save loading finishes
	function GameModeInitialResetCheck()
	{
		Parent::GameModeInitialCheck();

		//if there is no level list, attempt to create it
		if($SuperCreeper::numLevels == 0)
			SC_BuildLevelList();
		
		//if levellist is still empty, there are no levels
		if($SuperCreeper::numLevels == 0)
		{
			echo("\c6No Super Creeper levels available!");
			return;
		}

		if($SuperCreeper::Initialized)
			return;

		$SuperCreeper::Initialized = true;
		$SuperCreeper::CurrentLevel = -1;
				
		SC_NextLevel();
	}

	//when we're done loading a new level, reset the minigame
	function ServerLoadSaveFile_End()
	{
		Parent::ServerLoadSaveFile_End();
		
		//new level has loaded, reset minigame
		if($DefaultMiniGame.numMembers > 0) //don't bother if no one is here (this also prevents starting at round 2 on server creation)
			$DefaultMiniGame.scheduleReset(); //don't do it instantly, to give people a little bit of time to ghost
	}
	function MiniGameSO::Reset(%obj, %client)
	{
        if(!$Creep::Init) {
            SC_CancelPostGameSchedules();
            
            // clean up the previous round's mess
            if($Creep::active) {
                stopCreeper();
            }
            
            ClearCreeper();
            
            //make sure this value is an number
            $Creep::RoundLimit = mFloor($Creep::RoundLimit);

            //count number of minigame resets, when we reach the limit, go to next level
            if(%obj.numMembers >= 0)
            {
                $SuperCreeper::ResetCount++;
            }
            
            
            if($Creep::UseRounds) {
                if($SuperCreeper::ResetCount > $Creep::RoundLimit)
                {
                    $SuperCreeper::ResetCount = 0;
                    SC_NextLevel();
                }
                else
                {
                    messageAll('', "\c6Beginning round \c4" @ $SuperCreeper::ResetCount @ " \c6of \c4" @ $Creep::RoundLimit);
                    
                    Parent::Reset(%obj, %client);
                }
            } else {
                messageAll('', "\c6Beginning round \c4" @ $SuperCreeper::ResetCount);
                
                Parent::Reset(%obj, %client);
            }
            
            // get round type
            $Creep::RoundType = getSpecialRound();
            
            if($Creep::RoundType != 0) {
                switch($Creep::RoundType) {
                    case 1:
                        MessageAll('MsgAdminForce',"\c0NIGHTMARE ROUND\c6: The Creeper will go directly for players - and have a lot more health!");
                    case 2:
                        MessageAll('MsgAdminForce',"\c2INSTA-GIB ROUND\c6: The Creeper will grow a lot faster - but won't take much to kill!");
                    case 3:
                        MessageAll('MsgAdminForce',"\c3IMMUNITY ROUND\c6: The Creeper cannot be stopped - only slowed down!");
                }
            }
            
            schedule(33, 0, SC_PostGameSchedules);
        } else {
            messageAll('', "\c6The Creeper Gamemode has been initialized.");
            messageAll('',"\c6Start a game with \c4/nextlevel \c6or \c4/setlevel \c6to begin.");
        }
	}
    
    function GameConnection::spawnPlayer(%client) {
        Parent::spawnPlayer(%client);
        %client.updateSuperCreeperScore();
        
        if(!isObject(%client.player)) return; // slayer removes late joining players and this can cause errors here
        
        %player = %client.player;
        
        // CREEPER VARIABLES // 
        %client.kills = 0;
        %player.nochase = 0;
        
        // START ITEMS //
        // Anti-Creeper Loadout:
        if($Creep::GameMode == 0 || $Creep::GameMode == 3 || $Creep::GameMode == 5) {
            // are we using a map with shop functionality?
            if($Creep::UseShop) {
                switch(UpgradeMan.getValue(%client.bl_id, "CreepKillLvl")) {
                    case 1:
                        %player.addItem(nameToID(CreepKillWeakGunItem),%client);
                    case 2:
                        %player.addItem(nameToID(CreepKillGunItem),%client);
                    case 3:
                        %player.addItem(nameToID(CreepKillStrongGunItem),%client);
                    case 4:
                        %player.addItem(nameToID(CreepKillVStrongGunItem),%client);
                }
                
                if(UpgradeMan.getValue(%client.bl_id, "CreepBombLvl") > 0 && !$Creep::NoBombs) {
                    if(UpgradeMan.getValue(%client.bl_id, "CreepBombLvl") == 1) {
                        %player.addItem(nameToID(CreepBombItem),%client);
                    } else {
                        %player.addItem(nameToID(CreepBomb2Item),%client);
                    }
                }
            } else { // if not just give everyone the bog standard
                %player.addItem(nameToID(CreepKillGunItem),%client);
            }
        }
        
        // Creeper Planter Loadout:
        // not in yet
        
        // Competitive Loadout:
        // not in yet
        
        // That Bastard Loadout:
        // not in yet
        
        
        // LIVES //
        
        // can players respawn in the current gamemode?
        if($Creep::GameMode != 3 && $Creep::GameMode != 4 && $Creep::GameMode != 5) {
            %client.setLives(1); // if so, one life
        }
        
        // LOCATION //
        // players spawn on bricks named "spawn"
        %pos = getRandomPosOnBrick(getRandomSpawnBrick());
        %player.settransform(%pos);
        %player.setvelocity("0 0 0");
        return;
    }
    
    // packaging slayer functions because yoloswag
    function Slayer_MinigameSO::endRound(%this,%winner,%resetTime) {
        // clean up the previous round's mess
        if($Creep::active) {
            stopCreeper();
        }
        
        ClearCreeper();
        
        SC_CancelPostGameSchedules();
        
        // is anyone still alive?
        for(%i=0; %i < %this.numMembers; %i++)
        {
            %cl = %this.member[%i];
            if(!%cl.Dead())
            {
                // give the living players their reward
                if($Creep::GameMode == 0) {
                    messageAll('', "\c6- \c4" @ %cl.name SPC "\c6SURVIVED with" SPC %cl.kills SPC "kills. He receives\c4" SPC %cl.kills*5 SPC "\c6points.");
                    UpgradeMan.incValue(%cl.bl_id, "score", $Creep::RoundType > 0 ? %cl.kills*15 : %cl.kills*5);
                } else if($Creep::GameMode == 1) {
                    messageAll('', "\c6- \c4" @ %cl.name SPC "\c6SURVIVED. He receives\c4 200 \c6points.");
                    UpgradeMan.incValue(%cl.bl_id, "score", 200);
                }
            } else {
                messageAll('', "\c6- \c4" @ %cl.name SPC "\c6died with" SPC %cl.kills SPC "kills. He receives\c4" SPC mFloor(%cl.kills*0.5) SPC "\c6points.");
                UpgradeMan.incValue(%cl.bl_id, "score", mFloor(%cl.kills*0.5));
            }
            
            %cl.updateSuperCreeperScore();
        }
        
        Parent::endRound(%this,%winner,%resetTime);
    }
    
    function Slayer_MinigameSO::victoryCheck_Lives(%this)
    {
        // since creeper games work differently to normal TDMs, we need to overwrite this
        
        if(%this.isResetting())
            return -1;
            
        // if we're playing Outbreak or Karoshi, we need to check for a Creeper Victory
        if($Creep::GameMode == 4 || $Creep::GameMode == 5) {
            %this.VictoryCheck_Creeper();
        }
        
        // is everyone still alive? This doesn't matter if lives are off.
        for(%i=0; %i < %this.numMembers; %i++)
        {
            %cl = %this.member[%i];
            if(!%cl.Dead())
            {
                %count ++;
            }
        }
        
        if($creep::gamemode != 1 && $creep::gamemode != 3 && $creep::gamemode != 5) {
            if(!%count > 0) {
                return $DefaultMiniGame.Teams.getTeamFromName("The Creeper");
            } else {
                return -1;
            }
        } else {
            if(!%count > 1) {
                for(%i=0; %i < %this.numMembers; %i++)
                {
                    %cl = %this.member[%i];
                    if(!%cl.Dead())
                    {
                        return %cl.getTeam();
                    }
                }
            } else {
                return -1;
            }
        }
    }
    
    // yes I know this doesn't need to be in the package but it's neater so shut up
    function Slayer_MinigameSO::victoryCheck_Creeper(%this)
    {
        // Little extension to slayer.
        // We need to check if the creeper has gotten big enough to win.
        if(CreepGroup.getCount() >= $Creep::RequiredAmmount) {
            $DefaultMiniGame.endRound($DefaultMiniGame.Teams.getTeamFromName("The Creeper"));
        } else {
            $DefaultMiniGame.BottomPrintAll("\c2The creeper needs to grow by\c3" SPC ($Creep::RequiredAmmount - CreepGroup.getCount()) SPC "\c2 bricks to win!",3);
        }
    }
    function GameConnection::AutoAdmincheck(%this)
    {
        %r = parent::AutoAdmincheck(%this);
        %this.InitialDatabaseCheck();
        %this.updateSuperCreeperScore();
        return %r;
    }
    function GameConnection::onConnectRequest(%this,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o,%p)
    {
        for(%i=0;%i<clientgroup.getcount();%i++)
        {
            if(%c $= clientgroup.getobject(%i).getplayername())
            {
                %multiclienting = 1;
            }
        }
        if(%multiclienting)
            %this.schedule(10,delete,"You can only have one client at this server. Sorry.");
        return Parent::onConnectRequest(%this,%a,%b,%c,%d,%e,%f,%g,%h,%i,%j,%k,%l,%m,%n,%o,%p);     
    }
};
activatePackage(GameModeSuperCreeperPackage);

function getRandomPosOnBrick(%brick)
{
    %box = %brick.getworldbox();
    %x = getrandom(0, (mabs(getword(%box, 0) - getword(%box, 3)) - 1.5) * 500) / 1000 * (getrandom(0, 1) ? -1 : 1);
    %y = getrandom(0, (mabs(getword(%box, 1) - getword(%box, 4)) - 1.5) * 500) / 1000 * (getrandom(0, 1) ? -1 : 1);
    %z = mabs(getword(%box, 2) - getword(%box, 5)) / 2;
    return vectoradd(%brick.getworldboxcenter(), %x SPC %y SPC %z);
}

function getRandomSpawnBrick()
{
    %brick = BrickGroup_888888.NTObject_["spawn", getRandom(0, BrickGroup_888888.NTObjectCount_["spawn"]-1)];
    return %brick;
}

function SC_PostGameSchedules()
{
    %time = $Creep::StartTime;
    
    if($Creep::GameMode != 4 && %Creep::GameMode != 6) { // the tool in Karoshi and That Bastard starts the creeper automatically.
        $SC::PostGameSchedule[1] = schedule(1, 0, messageAll, '', "\c6The Creeper spores have started to pollenate. You have approximately \c4" @ %time SPC "\c6seconds to prepare!");
        $SC::PostGameSchedule[2] = schedule(%time*1000, 0, startCreeper);
        $SC::PostGameSchedule[3] = schedule((%time*1000)+1000, 0, messageAll, '', "\c6The Creeper is spreading! Run for your lives!");
    }
}

function SC_CancelPostGameSchedules() {
    for(%a = 0; %a < 4; %a++) {
        %schedule = $SC::PostGameSchedule[%a];
		if(!%schedule)
		{
			break;
		}
		cancel(%schedule);
    }
}

// import the addItem code
function Player::addItem(%player,%image,%client)
{
   for(%i = 0; %i < %player.getDatablock().maxTools; %i++)
   {
      %tool = %player.tool[%i];
      if(%tool == 0)
      {
         %player.tool[%i] = %image;
         %player.weaponCount++;
         messageClient(%client,'MsgItemPickup','',%i,%image);
         break;
      }
   }
}

function Player::removeItem(%this,%item)
{
   if(!isObject(%this) || !isObject(%item.getID()))
      return;
   for(%i=0;%i<%this.getDatablock().maxTools;%i++)
   {
      if(!isObject(%this.tool[%i])) continue;
      // ^ this line of code not being in the mod i ripped it from was the reason that mod caused console errors
      
      %tool=%this.tool[%i].getID();
      if(%tool==%item.getID())
      {
         %this.tool[%i]=0;
         messageClient(%this.client,'MsgItemPickup','',%i,0);
         if(%this.currTool==%i)
         {
            %this.updateArm(0);
            %this.unMountImage(0);
         }
      }
   }
}

// player database
// this is the server code that interfaces with zebase.
if(!isObject(UpgradeMan))
{
	if(isFile("config/SuperCreeper/db_save.cs"))
	{
		echo("LOADING OLD DATABASE");
		exec("config/SuperCreeper/db_save.cs");
        echo(UpgradeMan.getCount());
		UpgradeMan.createDataSheet();
        echo(UpgradeMan.getCount());
		UpgradeMan.sortlist();
        echo(UpgradeMan.getCount());
	}
	else
	{
		new ScriptGroup(UpgradeMan)
		{
			class = Zebase;
			datacount = 0;
			valuecount = 0;
			debug = 0;
		};
        UpgradeMan.registerValue("totalCreepKills", 0);
		UpgradeMan.registerValue("score", 0);
        UpgradeMan.registerValue("creepKillLvl", 1);
        UpgradeMan.registerValue("creepBombLvl", 0);
        
		UpgradeMan.createDataSheet();
	}
}

function SaveDatabase()
{
	cancel($Creep::SaveDBTick);
	UpgradeMan.save("config/SuperCreeper/db_save.cs");
	$Creep::SaveDBTick = schedule(300000, 0, SaveDatabase);
}

    $Creep::SaveDBTick = schedule(300000, 0, SaveDatabase);

function GameConnection::InitialDatabaseCheck(%this) {
    if(isObject(UpgradeMan.data[%this.bl_id])) {
        MessageClient(%this,'',"\c6Welcome back," SPC %this.name @ ". You have\c4" SPC UpgradeMan.getValue(%this.bl_id, "score") SPC "\c6Points. (" @ UpgradeMan.getValue(%this.bl_id, "totalCreepKills") SPC "CreepKills)");
        return;
    }
    
    MessageClient(%this,'',"\c6Welcome," SPC %this.name @ ". This is your first time playing.");
    UpgradeMan.createData(%this.bl_id);
}

function GameConnection::UpdateSuperCreeperScore(%this) {
    %this.score = UpgradeMan.getValue(%this.bl_id, "score");
}

function getSpecialRound() {
    %rand = getRandom(0, 49);
    
    if(%rand < 40) {
        return 0;
    } else {
        // special round!
        // 1 = nightmare, 2 = instagib, 3 = unstoppable creeper
        if(%rand < 43) {
            return 1;
        }
        else if(%rand < 46) {
            return 2;
        }
        else {
            return 2; //return 3; DISABLED BECAUSE BROKEN!
        }
    }
}

schedule(10000, 0, SC_NextLevel);