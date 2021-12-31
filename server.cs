//This will make making scripted brick based games much easier to do
//The idea is that a centralized event controls and sends signals to it's game that is all based within ts
function eventGameInit()
{
    if(!isObject(EventGameHandler))
    {
        %new = new ScriptGroup(){
            class = "EventGameHandler";
        };
        %new.setname("EventGameHandler");
        MissionCleanup.add(EventGameHandler);
    }
    $Server::EventGame::Initliazed = true;
}

if(!$Server::EventGame::Initliazed)
{
    eventGameInit();
}

exec("add-ons/event_games/src/eventGameHandler.cs");
exec("add-ons/event_games/src/eventGameType.cs");
exec("add-ons/event_games/src/misc.cs");
exec("add-ons/event_games/games/EventGame_TexasHoldem/texasHoldem.cs");
exec("add-ons/event_games/support/math.cs");
exec("add-ons/event_games/support/welfare.cs");
