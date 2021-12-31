//events
registerOutputEvent("fxDTSBrick","setColorColorBoard","",true);
registerOutputEvent("fxDTSBrick","paintColorColorBoard","",true);

function fxDTSBrick::setColorColorBoard(%brick,%client)
{
    %client.colorColorBoard = %brick.colorid;
}
function fxDTSBrick::paintColorColorBoard(%brick,%client)
{
    %brick.setcolor(%client.colorColorBoard + 0);
}