function fxdtsbrick::messageall(%brick,%text,%client)
{
	messageall('',strReplace(%text,"%2",%client.name));
}
registerOutputEvent("fxDtsBrick","messageall","string 200 200",1);


function givePlayerPoints(%client,%points)
{
	%client.setScore(%points = %client.getScore());
	%client.setScore(%client.score);


	$pref::server::points[%client.bl_id] = %client.score;
	//%client.setScore($pref::server::points[%client.bl_id]);
	messageclient(%client,'MsgAdminForce',"\c3You got \c6" @ %points @ " points;\c3 you now have \c6" @ $pref::server::points[%client.bl_id] @ "\c3 points.");
}

registerInputEvent("fxDTSBrick","onBuyItem","self fxDTSBrick" TAB "Player Player" TAB "Client GameConnection" TAB "Minigame Minigame");
function fxDTSBrick::onBuyItem(%brick,%client,%player)
{
    $inputTarget_Self = %brick;
	$inputTarget_Player = %player;
	$inputTarget_Client = %client;
	$inputTarget_Vehicle = %brick.vehicle;
	$inputTarget_Minigame = getMinigameFromObject(%client);
}


function fxDTSBRICK::buyItem(%brick,%item,%amount,%client)
{
	$pref::server::points[%client.bl_id] = %client.getScore();
	%rebuy = false;

	if($pref::server::boughtItem[%client.bl_id,%item])
	{
		messageclient(%client,'',"\c3Here's a free rebuy");
		%rebuy = true;
	}
    else
    {
        if($pref::server::points[%client.bl_id] $= "")
		$pref::server::points[%client.bl_id] = 0;

        if($pref::server::points[%client.bl_id] < %amount)
        {
            messageclient(%client,'',"\c3You can't afford the\c6" @ %item.uiName @ "\c3 it costs \c6" @ %amount @ "\c3 points and you only have \c6" @ $pref::server::points[%client.bl_id] @ "\c3 points.");
            return;
        }

    }

    %maxWeapons = %client.player.getDatablock().maxWeapons;
    for(%i = 0; %i < %maxWeapons; %i++)
    {
		if(%client.player.tool[%i] == %item)
		{
			break;
		}

        if(!isObject(%client.player.tool[%i]))
        {
			

			if(!%rebuy)
			{
				$pref::server::boughtItem[%client.bl_id,%item] = true;

				$pref::server::points[%client.bl_id] -= %amount;
				%client.setScore($pref::server::points[%client.bl_id]);
			}
            

            %client.player.tool[%i] = %item.getID();
            messageClient(%client,'MsgItemPickup','',%i,%item.getID());
	        messageclient(%client,'',"\c3You just bought the \c6" @ %item.uiName @ "\c3 for \c6" @ %amount @ "\c3 points and have \c6" @ $pref::server::points[%client.bl_id] @ "\c3 points remaining!");
            %brick.processInputEvent("onBuyItem", %client);
			%bought  = true;
            break;
        }
    }

    if(!%bought)
    {
        messageClient(%client,'', "\c3You do not have enough room for this item.");
    }
}

registerOutputEvent("fxDTSBRick","buyItem","datablock ItemData\tstring 128 100",1);


package shopEquip
{
	function ServerCmdDropTool (%client, %position)
	{
		if(%client.isAdmin)
		{
			parent::ServerCmdDropTool (%client, %position);
		}
		else
		{
			%client.player.tool[%position] = 0;
        	messageClient(%client,'MsgItemPickup','',%position,"");
		}
	}

	function Player::GiveDefaultEquipment(%player)	
	{
		%client = %player.client;

		if((%client.waitingForFallingTiles && $fallingTilesSpawnTime) || %player.inFallingTiles)
		{
			for(%b=0;%b<5;%b++)
			{
				%client.player.tool[%b] = 0;
				messageClient(%client,'MsgItemPickup','',%b,-1);
			}
			return;
		}

		if(%player.client.inKnifeDM || %player.inFallingTiles || %player.client.inTournament["boxing"])
		{
			return parent::givedefaultequipment(%player);
		}
		else
		{
			if(%client.isAdmin || %client.isSuperAdmin)
			{
				messageclient(%client,'',"You weren't given shop items because you're an admin!");
				return parent::givedefaultequipment(%player);
			}

			%player.setDatablock(playerNoJet);
		}
	}
};
deactivatepackage(shopEquip);
activatepackage(shopEquip);




