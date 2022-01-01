$Pref::NYWelfare::Ammount = 20;
$Pref::NYWelfare::Time = 60000;
cancel($NYwelfareSchedule);
$NYwelfareSchedule = schedule(10000,0,"NYWelfareCheck");

package NY2022WelfareCheck
{
    function NY2022WelfareCheck()
    {
        talk("suck");
    }

    function NYWelfareCheck()
    {
    cancel($NYwelfareSchedule);

    %clientCount = clientGroup.getCount();


    for(%i = 0; %i < %clientCount; %i++)
    {
        %client = clientGroup.getObject(%i);
        NY2022WelfareGiveWelfare(%client);
    }

    $NYwelfareSchedule = schedule($Pref::NYWelfare::Time,0,"NYWelfareCheck");
    }

    function NYWelfareGiveWelfare(%client)
    {
    %score = %client.score;
    %ammount = $Pref::NYWelfare::Ammount;

    if(%score < %ammount)
    {
        %client.chatMessage("\c2You have recieved welfare for being broke. (" @ %ammount @ " points)");
        %client.setScore(%ammount);
    }
    }


    function GameConnection::spawnPlayer(%client)
    {
        if(!%client.hasSpawnedOnce)
        {
            NYWelfareGiveWelfare(%client);
        }
        
        return parent::spawnPlayer(%client);
    }
};
deactivatePackage("NY2022WelfareCheck");
activatePackage("NY2022WelfareCheck");