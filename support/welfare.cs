$Pref::NYWelfare::Ammount = 10;
$Pref::NYWelfare::Time = 60000;

$NYwelfareSchedule = shcedule(10000,0,"NY2022WelfareCheck");

package NY2022WelfareCheck
{
    function NY2022WelfareCheck()
    {
        cancel($NYwelfareSchedule);

        %clientCount = clientGroup.getCount();
        

        for(%i = 0; %i < %clientCount; %i++)
        {
            %client = clientGroup.getObject(%i);
            NY2022WelfareGiveWelfare(%client);
        }

        $NYwelfareSchedule = shcedule($Pref::NYWelfare::Time,0,"NY2022WelfareCheck");
    }

    function NY2022WelfareGiveWelfare(%client)
    {
        %score = %client.score;
        %ammount = $Pref::NYWelfare::Ammount;

        if(%score < %ammount)
        {
            %client.chatMessage("\c2You have recieved welafare for being broke. (" @ %ammount @ " points)");
            %client.setScore(%ammount);
        }
    }

    function GameConnection::spawnPlayer(%client)
    {
        if(!%client.hasSpawnedOnce)
        {
            NY2022WelfareGiveWelfare(%client);
        }
        
        return parent::spawnPlayer(%client);
    }
};
deactivatePackage("NY2022WelfareCheck");
activatePackage("NY2022WelfareCheck");