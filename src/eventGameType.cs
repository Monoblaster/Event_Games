function EventGameType::OnAdd(%this)
{
    EventGameHandler.AddGame(%this);
}

function EventGameType::DoCommand(%this,%gameName,%gameCommand,%parameters,%brick,%client)
{
    if((%i = $Server::EventGame::Game[%this.class,%gameName]) !$= "")
    {
        %game = %this.getObject(%i);
        retrieveEventGameParameters(%game,%parameters);
        %game.EventGame_Call(%gameCommand,%brick,%client);
    }
}

function EventGameType::DoServerGameCommand(%this,%game,%gameCommand,%parameters,%client)
{
    if(isFunction(%game.class,"ServerGame" @ %gameCommand))
    {
        retrieveEventGameParameters(%game,%parameters);
        %game.EventGame_Call("ServerGame" @ %gameCommand,%client);
        return true;
    }
    return false;
}