$count = 0;
$Server::EventGame::Function["EventGameType_All",$count++ - 1] = "NewGame";
$Server::EventGame::Function["EventGameType_All",$count++ - 1] = "EndGame";
$Server::EventGame::Function["EventGameType_All",$count++ - 1] = "RestartGame";
$Server::EventGame::Function["EventGameType_All",$count++ - 1] = "AddPlayer";
$Server::EventGame::Function["EventGameType_All",$count++ - 1] = "RemovePlayer";
$Server::EventGame::Count["EventGameType_All"] = $count;


function EventGameHandler::AddGame(%this,%eventGame)
{
    %name = %eventGame.class;
    %uiName = %eventGame.uiName;
    %count = $Server::EventGame::Game[C] + 0;
    if(!$Server::EventGame::Game[I,%name])
    {
        %this.add(%eventGame);
        $Server::EventGame::Game[I,%name] = %count;
        $Server::EventGame::Game[N,%count] = %name;
        $Server::EventGame::Game[C]++;

        updateEventGameeventList();
    }
    else
    {
        eventGameWarn(%uiName @ " group is already defined");
        %eventGame.delete();
    }
}

function EventGameHandler::DoCommand(%this,%group,%gameName,%gameCommand,%parameters,%brick,%client)
{
    if(isFunction("EventGameHandler",%gameCommand))
    {
        %this.EventGame_Call(%gameCommand,%group,%gameName,%brick,%client);
    }
    
    %group.DoCommand(%gameName,%gameCommand,%parameters,%brick,%client);
}

function EventGameHandler::NewGame(%this,%group,%gameName,%brick,%client)
{
    if((%i = $Server::EventGame::Game[%group.class,%gameName]) !$= "")
    {
        EventGameHandler.DoCommand(%group,%gameName,"EndGame","",%brick,%client);
    }

    %new = new scriptObject()
    {
        class = %group.game;
        name = %gameName;
    };

    $Server::EventGame::Game[%group.class,%gameName] = %group.getCount();
    %group.add(%new);
    
    
    %brick.currEventGame = %new;
    %new.currBrick = %brick;
    %brickGroup = %brick.getGroup();
    collectEventGameBricks(%new,%brickgroup);

    return true;
}

function EventGameHandler::EndGame(%this,%group,%gameName,%brick,%client)
{
    if((%i = $Server::EventGame::Game[%group.class,%gameName]) !$= "")
    {
        %game = %group.getObject(%i);

        %count = %game.playerCount;
        for(%j = 0; %j < %count; %j++)
        {
            %currClient = %game.player[%j];
            %this.DoCommand(%group,%gameName,"RemovePlayer","",%brick,%currClient);
        }
        $Server::EventGame::Game[%group.class,%gameName] = "";
        %game.delete();

        %brick.currEventGame = "";

        %count = %group.getCount();
        for(%j = %i; %j < %count; %j++)
        {
            %name = %group.getObject(%j);
            $Server::EventGame::Game[%group.class,%name] = %j;
        }

        return true;
    }

    return false;
}

function EventGameHandler::AddPlayer(%this,%group,%gameName,%brick,%client)
{
    if((%i = $Server::EventGame::Game[%group.class,%gameName]) !$= "")
    {
        %game = %group.getObject(%i);

        %currGame = %client.currEventGame;

        if(%currGame)
        {
            //TODO: send error to player
            return false;
        }

        %client.currEventGame = %game;
        
        %count = %game.playerCount + 0;
        %game.player[%count] = %client;
        %game.playerIndex[%client] = %count;
        %game.playerCount++;
    
        return true;
    }

    return false;
}

function EventGameHandler::RemovePlayer(%this,%group,%gameName,%brick,%client)
{
    if((%i = $Server::EventGame::Game[%group.class,%gameName]) !$= "")
    {
        %game = %group.getObject(%i);

        %currGame = %client.currEventGame;

        if(!%currGame)
        {
            //TODO: send error to player
            return false;
        }

        %client.currEventGame = "";
        %i = %game.playerIndex[%client];
        %count = %game.playerCount + 0;
        for(%j = %i; %j < %count; %j++)
        {
            %count.player[%j] = %count.player[%j + 1];
        }
        %game.playerCount--;
        
        return true;
    }

    return false;
}

function EventGameHandler::DoServerGameCommand(%this,%group,%game,%gameCommand,%parameters,%client)
{
    %isCommand = false;
    if(isFunction("EventGameHandler","ServerGame" @ %gameCommand))
    {
        %this.EventGame_Call("ServerGame" @ %gameCommand,%group,%game,%client);
        %isCommand = true;
    }
    %isCommand = %isCommand || %group.DoServerGameCommand(%game,%gameCommand,%parameters,%client);

    return %isCommand;
}

function EventGameHandler::serverGameLeave(%this,%group,%game,%client)
{
    %client.chatMessage("You have left the game");
    %this.DoCommand(%group,%game.name,"RemovePlayer","",%game.currBrick,%client);
}

package EventGameHandler
{
    function serverCmdMessageSent(%client, %text)
    {
        if(strPos(%text,"!") == 0 && (%obj = %client.currEventGame))
        {
            %text = getSubStr(%text,1,strLen(%text) - 1);
            %gameCommand = getWord(%text,0);
            %parameters = getWords(%text,1);
            %return = EventGameHandler.DoServerGameCommand(%obj.getGroup(),%obj,%gameCommand,%parameters,%client);

        }
        else
        {
            return parent::serverCmdMessageSent(%client, %text);
        }
        
    }
    function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc)
    {
        %eventGame = %client.currEventGame;
        if(%eventGame)
        {
            EventGameHandler.DoCommand(%eventGame.getGroup(),%eventGame.name,"RemovePlayer","",0,%client);
        }
        return parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);
    }
    function GameConnection::onDrop(%client, %reason)
    {
        %eventGame = %client.currEventGame;
        if(%eventGame)
        {
            EventGameHandler.DoCommand(%eventGame.getGroup(),%eventGame.name,"RemovePlayer","",0,%client);
        }
        return parent::onDrop(%client, %reason);
    }
};
deactivatePackage("EventGameHandler");
activatePackage("EventGameHandler");