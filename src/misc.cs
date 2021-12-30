$EventGame_Call_Lookup = 0;
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "();";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1,%v2);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1,%v2,%v3);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1,%v2,%v3,%v4);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1,%v2,%v3,%v4,%v5);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1,%v2,%v3,%v4,%v5,%v6);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1,%v2,%v3,%v4,%v5,%v6,%v7);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1,%v2,%v3,%v4,%v5,%v6,%v7,%v8);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1,%v2,%v3,%v4,%v5,%v6,%v7,%v8,%v9);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1,%v2,%v3,%v4,%v5,%v6,%v7,%v8,%v9,%v10);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1,%v2,%v3,%v4,%v5,%v6,%v7,%v8,%v9,%v10,%v11);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1,%v2,%v3,%v4,%v5,%v6,%v7,%v8,%v9,%v10,%v11,%v12);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1,%v2,%v3,%v4,%v5,%v6,%v7,%v8,%v9,%v10,%v11,%v12,%v13);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1,%v2,%v3,%v4,%v5,%v6,%v7,%v8,%v9,%v10,%v11,%v12,%v13,%v14);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1,%v2,%v3,%v4,%v5,%v6,%v7,%v8,%v9,%v10,%v11,%v12,%v13,%v14,%v15);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1,%v2,%v3,%v4,%v5,%v6,%v7,%v8,%v9,%v10,%v11,%v12,%v13,%v14,%v15,%v16);";
$EventGame_Call_Lookup[$EventGame_Call_Lookup++ - 1] = "(%v0,%v1,%v2,%v3,%v4,%v5,%v6,%v7,%v8,%v9,%v10,%v11,%v12,%v13,%v14,%v15,%v16,%v17);";

function SimObject::EventGame_Call(%this, %method, %v0, %v1, %v2, %v3, %v4, %v5, %v6,%v7, %v8, %v9, %v10, %v11, %v12, %v13, %v14, %v15, %v16, %v17)
{
    %class = %this.class;

    if(%class $= "")
    {
        %class = %this.getClassName();
    }

	if(!isFunction(%class,%method))
    {
        eventGameWarn("event game function" SPC %class SPC %method SPC "does not exist");
		return"";
    }

	%numArguments = 0;

	for(%i = 0; %i < 18; %i++)
	{
		if(%v[%i] !$= "")
			%numArguments = %i + 1;
	}

	return eval(%this @ "." @ %method @ $EventGame_Call_Lookup[%numArguments]);
}

function fxDTSBrick::gameSetNode(%brick, %name, %offset, %eulerRotation)
{
    %radRotation = %eulerRotation * ($PI / 180);
    %brick.NodeTable[%name] = %offset SPC vectorToRotUp(vectorRotate("1 0 0", "0 0 1", %radRotation));
}

function fxDTSBrick::gameGetNode(%brick,%name)
{
    return %brick.nodeTable[%name];
}

 function EventGame::gameBrickFunction(%game,%name,%function, %v0, %v1, %v2, %v3, %v4, %v5, %v6,%v7, %v8, %v9, %v10, %v11, %v12, %v13, %v14, %v15, %v16)
    {
        %brick = %game.NT[%game.name @ %name];
        if(isObject(%brick))
        {
            %brick.EventGame_Call(%function, %v0, %v1, %v2, %v3, %v4, %v5, %v6,%v7, %v8, %v9, %v10, %v11, %v12, %v13, %v14, %v15, %v16);
        }
        else
        {
           eventGameWarn("brick" SPC %game.name @ %name SPC "does not exist");
           backtrace();
        }
    }

    function EventGame::chatMessageToPlayers(%game,%msg)
    {
        %playerCount = %game.playerCount;
        for(%i = 0; %i < %playerCount; %i++)
        {
            %client = %game.getIndexPlayer(%i);
            %client.chatMessage(%msg);
        }
    }

    function EventGame::playSoundToPlayers(%game,%sound)
    {
        %playerCount = %game.playerCount;
        for(%i = 0; %i < %playerCount; %i++)
        {
            %client = %game.getIndexPlayer(%i);
            %client.play2d(%sound);
        }
    }

    function EventGame::getIndexPlayer(%game,%index)
    {
        return %game.indexPlayer[%index];
    }

    function EventGame::getPlayerIndex(%game,%player)
    {
        return %game.playerIndex[%player];
    }

    function EventGame::retrieveEventGameParameters(%game,%parameters)
    {
        %parameters = strReplace(%parameters,",","\t");
        %count = getfieldCount(%parameters);
        for(%i = 0; %i < 20; %i++)
        {
            %field = trim(getField(%parameters,%i));
            %game.p[%i] = %field;
        }
    }

    function EventGame::collectEventGameBricks(%game,%brickgroup)
    {
        %name = %game.name;

        %NTCount = %brickgroup.NT["NameCount"];
        for(%i = 0; %i < %NTCount; %i++)
        {
            %NTName = %brickgroup.NTName[%i];
            if(strPos(%NTName,%name) == 1)
            {
                %brick = %brickgroup.NTOBject[%NTName,0];
                %game.collectEventGameBrickParameters(%NTName);
                %game.NT[stripEventGameParameters(%NTName)] = %brick;
            }
        }
    }

    function EventGame::collectEventGameBrickParameters(%game,%name)
    {
        %strippedName = stripEventGameParameters(%name);
		%strippedParameters = strReplace(stripEventGameBrickName(%name),"APOS","\t");
        %count = getFieldCount(%strippedParameters);
        for(%i = 0; %i < %count; %i++)
        {
            %game.PT[%strippedName,%i] = strReplace(getField(%strippedParameters,%i),"DASH","-");
        }
        
    }

package EventGames
{
    function updateEventGameEventList()
    {
        %count = $Server::EventGame::Game[C];
        for(%i = 0; %i < %count; %i++)
        {
            %name = $Server::EventGame::Game[N,%i];
            %index = $Server::EventGame::Game[I,%name];
            %group = EventGameHandler.getObject(%index);
            %gameName = getSafeVariableName(%group.uiName);

            %functionList = "";
            %functionlistCount = 0;
            for(%j = 0; %j < 2; %j++)
            {
                %typenames[0] = "EventGameType_All";
                %typenames[1] = %name;
                
                %count = $Server::EventGame::Count[%typenames[%j]];
                for(%k = 0; %k < %count; %k++)
                {
                    %functionName = $Server::EventGame::Function["EventGameType_All",%k];
                    %functionList = trim(%functionList SPC %functionName SPC %functionlistCount);
                    $Server::EventGame::FunctionList[%name,%functionlistCount++ - 1] = %functionName;
                }
            }

            %evalString = "";
            %evalString = %evalString @ "function fxDTSBrick::EventGame" @ %gameName @ "(%brick,%gameName,%gameCommand,%parameters,%client)";
            %evalString = %evalString @ "{";
                %evalString = %evalString @ "EventGameHandler.DoCommand(" @ %group @ ",%gamename,$Server::EventGame::FunctionList[" @ %name @ ",%gameCommand],%parameters,%brick,%client);";
            %evalString = %evalString @ "}";

            eval(%evalString);
            registerOutputEvent("fxDTSBrick","EventGame" @ %gameName,"string 200 100" TAB "list" SPC %functionList TAB "string 200 200",true);
        }
        
        %count = clientGroup.getCount();
        for(%i = 0 ; %i < %count; %i++)
        {
            %client = clientGroup.getObject(%i);
            serverCmdRequestEventTables(%client);
        }
    }

	function stripEventGameParameters(%name)
    {
		%end = strPos(%name,"APOS") - 1;
		
		if(%end == -2)
		{
			%end = strLen(%name);
		}

        return getSubStr(%name,1, %end);
    }

	function stripEventGameBrickName(%name)
    {
		%start = strPos(%name,"APOS") + 4;
		
		if(%start == 3)
		{
			%start = strLen(%name);
		}

        return getSubStr(%name,%start,strLen(%name) - %start);
    }

    function eventGameWarn(%text)
    {
        warn("Event Games:" SPC %text @ ".");
    }
};
deactivatePackage("EventGames");
activatePackage("EventGames");
