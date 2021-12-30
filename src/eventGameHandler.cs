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
    if($Server::EventGame::Game[I,%name] $= "")
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
        %this.EventGame_Call(%gameCommand,%group,%gameName,%parameters,%brick,%client);
    }
    else
    {
        %group.DoCommand(%gameName,%gameCommand,%parameters,%brick,%client);
    }
    
}

function EventGameHandler::NewGame(%this,%group,%gameName,%parameters,%brick,%client)
{
    if((%i = $Server::EventGame::Game[%group.class,%gameName]) !$= "")
    {
        return;
    }

    %new = new scriptObject()
    {
        superClass = "EventGame";
        class = %group.game;
        name = %gameName;
    };

    $Server::EventGame::Game[%group.class,%gameName] = %group.getCount();
    %group.add(%new);
    
    
    %brick.currEventGame = %new;
    %new.currBrick = %brick;
    %brickGroup = %brick.getGroup();
    %new.collectEventGameBricks(%brickgroup);

    %group.DoCommand(%GameName,"NewGame",%parameters,%brick,%client);
}

function EventGameHandler::EndGame(%this,%group,%gameName,%parameters,%brick,%client)
{
    %group.DoCommand(%GameName,"EndGame",%parameters,%brick,%client);

    if((%i = $Server::EventGame::Game[%group.class,%gameName]) !$= "")
    {
        %game = %group.getObject(%i);

        %count = %game.playerCount;
        for(%j = %count - 1; %j >= 0; %j--)
        {
            talk(%j);
            %currClient = %game.getIndexPlayer(%j);
            %currClient.currEventGame = "";
            EventGameHandler.DoCommand(%group,%gameName,"RemovePlayer","",%brick,%currClient);
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
    }
}

function EventGameHandler::AddPlayer(%this,%group,%gameName,%parameters,%brick,%client)
{
    if((%i = $Server::EventGame::Game[%group.class,%gameName]) !$= "")
    {
        %game = %group.getObject(%i);

        %currGame = %client.currEventGame;

        if(%currGame)
        {
            %client.centerPrint("\c6You are already in a game. Type !leave and try again",2);
            return;
        }

        %client.currEventGame = %game;
        
        %count = %game.playerCount + 0;
        %game.IndexPlayer[%count] = %client;
        %game.playerIndex[%client] = %count;
        %game.playerCount++;
    
    }
    %group.DoCommand(%GameName,"AddPlayer",%parameters,%brick,%client);
}

function EventGameHandler::RemovePlayer(%this,%group,%gameName,%parameters,%brick,%client)
{
    %group.DoCommand(%gameName,"RemovePlayer",%parameters,%brick,%client);
    if((%i = $Server::EventGame::Game[%group.class,%gameName]) !$= "")
    {
        %game = %group.getObject(%i);

        %currGame = %client.currEventGame;

        if(%currGame != %game)
        {
            return;
        }

        %client.currEventGame = "";
        %i = %game.playerIndex[%client];
        %count = %game.playerCount + 0;
        for(%j = %i; %j < %count; %j++)
        {
            %player = %game.player[%j + 1];
            %game.indexPlayer[%j] = %player;
            %game.playerIndex[%player] = %j;
        }
        %game.playerCount--;
    }
}

function EventGameHandler::DoServerGameCommand(%this,%group,%game,%gameCommand,%parameters,%client)
{
    %isCommand = false;
    if(isFunction("EventGameHandler","ServerGameCmd" @ %gameCommand))
    {
        %this.EventGame_Call("ServerGameCmd" @ %gameCommand,%group,%game,%parameters,%client);
        %isCommand = true;
    }
    else
    {
        %isCommand = %group.DoServerGameCommand(%game,%gameCommand,%parameters,%client);
    }

    return %isCommand;
}

function EventGameHandler::serverGameCmdLeave(%this,%group,%game,%parameters,%client)
{
    %group.DoServerGameCommand(%game,"leave",%parameters,%client);
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