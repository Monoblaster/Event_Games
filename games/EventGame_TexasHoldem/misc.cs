function EventGame_TexasHoldem::GetNextSeat(%this,%currSeat)
{
    %seatCount = %this.numSeats;
    for(%i = 1; %i <= %seatCount; %i++)
    {
        %checkSeat = mod(%currSeat - %i,%seatCount);
        if(%this.seatInHand[%checkSeat] && !%this.seatFolded[%checkSeat] && !%this.seatAllIn[%checkSeat])
        {
            return %checkSeat;
        }
    }

    return -1;
}

function EventGame_TexasHoldem::GetRandomOccupiedSeat(%this)
{
    %playerCount = %this.playerCount;
    
    return %this.playerSeat[getRandom(0,%this.playerCount - 1)];
}

function EventGame_TexasHoldem::DealCard(%this,%brickName,%slot,%down,%delay)
{
    %currCard = gameBrickFunction(%this,%brickName,"getBrickCard",%slot);
    if(%currCard $= "")
    {
        %card = %this.deck.removeCard();

        if(!%delay)
        {
            %this.gameBrickFunction(%this,%brickName,"PlaceBRickCard",%slot,%card,%down);
        }
        else
        {
            schedule(%delay,%this,"gameBrickFunction",%this, %brickname, "placeBrickCard",%slot,%card,%down);
        }
        return %card;
    }
    return "";
}

function EventGame_TexasHoldem::RemoveCard(%this,%brickName,%slot,%delay)
{
    %card = gameBrickFunction(%this,%brickName,"getBrickCard",%slot);
    if(%card !$= "")
    {
        if(!%delay)
        {
            %this.gameBrickFunction(%this,%brickName,"removeBrickCard",%slot);
        }
        else
        {
            schedule(%delay,%this,"gameBrickFunction",%this, %brickname, "removeBrickCard",%slot);
        }
    }
}

function EventGame_TexasHoldem::PeakCards(%this,%seat)
{
    %card0 = %this.hand[%seat,0];
    %card1 = %this.hand[%seat,1];
    %player = %this.player[%this.seatPlayer[%seat]].player;

    if(!%player.peaking)
    {
        gameBrickFunction(%this, "hand" @ %seat, "removeBrickCard",0);
        gameBrickFunction(%this, "hand" @ %seat, "removeBrickCard",1);

        %player.peaking = true;
        %player.displayCards();
        %player.isCardsVisible = 0;
        bottomprintCardInfo(%player);
    }
}

function EventGame_TexasHoldem::UnPeakCards(%this,%seat)
{
    %card0 = %this.hand[%seat,0];
    %card1 = %this.hand[%seat,1];
    %player = %this.player[%this.seatPlayer[%seat]].player;
    
    if(%player.peaking)
    {
        %player.hideCards();
        %player.peaking = false;

        gameBrickFunction(%this, "hand" @ %seat, "placeBrickCard",0,%card0,true);
        gameBrickFunction(%this, "hand" @ %seat, "placeBrickCard",1,%card1,true);
    }
}

function fxDTSBrick::setupPlayerTexasHoldemDisplay(%brick,%game,%seat)
{
    %brick.seat = seat;

    %eulerRotation = %game.PT[stripEventGameParameters(%brick.getname()),0] + 0;
    %radRotation = %eulerRotation * ($PI / 180);
    %Center = vectorSub(%brick.getPosition(),"0 0 " @ %brick.dataBlock.brickSizeZ/ 10);

    %seperation = 0.19;

    //cards
    %brick.gameSetNode("card0", vectorAdd(vectorRotate("0.19 0 0","0 0 1", %radRotation),%center), %eulerRotation);
    %brick.gameSetNode("card1", vectorAdd(vectorRotate("-0.19 0 0","0 0 1", %radRotation),%center), %eulerRotation);
    //betted chips
    %brick.gameSetNode("bet", vectorAdd(vectorRotate("0.4 0.5 0","0 0 1",%radRotation),%center), %eulerRotation);

    //chip
    %brick.gameSetNode("chip", vectorAdd(vectorRotate("-0.3 0.6 0","0 0 1",%radRotation),%center), %eulerRotation);
}

function fxDTSBrick::setupCommunityCardsTexasHoldemDDisplay(%brick,%game)
{
    %eulerRotation = %game.PT[stripEventGameParameters(%brick.getname()),0] + 0;
    %radRotation = %eulerRotation * ($PI / 180);
    %Center = vectorSub(%brick.getPosition(),"0 0 " @ %brick.dataBlock.brickSizeZ/ 10);

    //cards
    %brick.gameSetNode("card0", vectorAdd(vectorRotate("0.8 0 0","0 0 1", %radRotation),%center), %eulerRotation);
    %brick.gameSetNode("card1", vectorAdd(vectorRotate("0.4 0 0","0 0 1", %radRotation),%center), %eulerRotation);
    %brick.gameSetNode("card2", vectorAdd("0 0 0",%center), %eulerRotation);
    %brick.gameSetNode("card3", vectorAdd(vectorRotate("-0.4 0 0","0 0 1", %radRotation),%center), %eulerRotation);
    %brick.gameSetNode("card4", vectorAdd(vectorRotate("-0.8 0 0","0 0 1", %radRotation),%center), %eulerRotation);
}

function fxDTSBrick::getBrickCard(%brick,%slot)
{
    return %brick.placedCard[%slot];
}

function fxDTSBrick::placeBrickCard(%brick, %slot, %card, %down) 
{
    %brick.removeBrickCard(%slot);
    %brickName = %brick.getName();
    serverPlay3D(("cardPlace" @ getRandom(1, 4) @ "Sound"), %brick.getPosition());
	
	%cardShape = new StaticShape(CardShapes) {
		dataBlock = CardShape;
		card = %card;
	};
    %brick.placedCard[%slot] = %cardShape;

    %handStrPos = striPos(%brickName,"hand");
    if(%handStrPos != -1)
    {
        %cardShape.owningSeat = getSubStr(%brickName, %handStrPos + 4, 1);
    }

    %cardShape.brick = %brick;
	%cardShape.setTransform(%brick.gameGetNode("card" @ %slot));

	if (!%down) {
		%cardShape.playThread(0, cardFaceUp);
	} else {
		%cardShape.playThread(0, cardFaceDown);
	}

	%cardShape.down[%slot] = %down;

	cardDisplay(%cardShape, getCardName(%card));
    //disables card flipping
    %cardShape.card = "";
}

function fxDTSBrick::removeBrickCard(%brick,%slot)
{
    %card = %brick.placedCard[%slot];
    if(%card)
    {
        serverPlay3D(("cardPick" @ getRandom(1, 4) @ "Sound"), %brick.getPosition());
        %brick.placedCard[%slot] = "";
        %card.delete();
    }
}

function fxDTSBrick::flipBrickCard(%brick,%slot)
{
    %card = %brick.placedCard[%slot];
    if(%card)
    {
        serverPlay3D(("cardPick" @ getRandom(1, 4) @ "Sound"), %brick.getPosition());
        %down = %card.down;
        if (%down) {
            %card.playThread(0, cardFaceUp);
        } else {
            %card.playThread(0, cardFaceDown);
        }
    }
}

function fxDTSBrick::createBrickChips(%b,%value)
{
	if (%value <= 0) {
		%b.removeBrickChips();
		return;
	}
	%loc = %b.gameGetNode("bet");

	%chipVector = getChipCounts(%value);
	%count = 0;
	%largestChipCount = 0;
	for (%i = 0; %i < getWordCount(%chipVector); %i++) {
		%chipCount = getWord(%chipVector, %i);
		if (!isObject(%b.chip[%count]) && %chipCount != 0) {
			%b.chip[%count] = new StaticShape(ChipDisplayShapes) {
				datablock = ChipShape;
			};
		} else if (isObject(%b.chip[%count]) && %chipCount == 0) {
			%b.chip[%count].delete();
		}

		if (!isObject(%b.chip[%count])) {
			continue;
		}
		%b.chip[%count].setNodeColor("ALL", $ChipType[%i]);
		%b.chip[%count].setScale("1 1 " @ %chipCount);
		// %b.chip[%count].chipValue = %chipCount * $ChipType[%i @ "Cost"];

		// %b.chip[%count].setShapeNameColor(getWords($ChipType[%i], 0, 2));
		// %b.chip[%count].setShapeName(%chipCount @ " x " @ $ChipType[%i @ "Cost"]);

		if (%chipCount > %largestChipCount) {
			%largestChipCount = %chipCount;
			%largest = %count;
		}
		%b.chip[%count].setShapeName("");
		%b.chip[%count].setShapeNameColor("1 1 1");

		%b.chip[%count].setTransform(vectorAdd($offset[%count], %loc));
		%count++;
	}

	for (%i = %count; %i < getWordCount(%chipVector); %i++) {
		if (isObject(%b.chip[%i])) {
			%b.chip[%i].delete();
		}
	}

	if (isObject(%b.chip[%largest])) {
		%b.chip[%largest].setShapeName(%value);
	}

	%b.isDisplayingChips = 1;
}

function fxDTSBrick::removeBrickChips(%b) {
	for (%i = 0; %i < 10; %i++) {
		if (isObject(%b.chip[%i])) {
			%b.chip[%i].delete();
		}
	}

	%b.isDisplayingChips = 0;
}

function fxDTSBrick::createBrickDealerChip(%brick,%type)
{
    if(%brick.dealerChip)
    {
        %brick.dealerChip.delete();
        %brick.dealerChip = "";
    }

    if(%type == 0)
    {
        return;
    }

    %brick.dealerChip = new StaticShape(ChipDisplayShapes) {
        datablock = ChipShape;
    };
    %color[1] = "0.7 0.7 0.7 1";
    %color[2] = "1 0 0 1";
    %color[3] = "1 0 0 1";
    %scale[1] = "1.5 1.5 1";
    %scale[2] = "1.5 1.5 1";
    %scale[3] = "1.5 1.5 2";

    %brick.dealerChip.setNodeColor("ALL", %color[%type]);
    %brick.dealerChip.setScale(%scale[%type]);

    %brick.dealerChip.setTransform(%brick.gameGetNode("Chip"));
}

package EventGame_texasHoldem
{
    function Armor::onTrigger(%this, %obj, %trig, %val) 
    {
        %player = %obj;
		%client = %player.client;
		%s = getWords(%obj.getEyeTransform(), 0, 2);
		%masks = $TypeMasks::fxBrickObjectType | $TypeMasks::StaticObjectType | $TypeMasks::TerrainObjectType;

        %ev = %client.currEventGame;
        if  (%ev.class $= "EventGame_TexasHoldem")
        {
            %seat = %ev.playerSeat[%ev.playerIndex[%client]];
            if (%trig == 0 && %val == 1) 
            {
                if(%ev.betting)
                {
                    if (!%player.peaking) 
                    { //Flip card near hit location
                        %e = vectorAdd(vectorScale(%obj.getEyeVector(), 5), %s);
                        %ray = containerRaycast(%s, %e, %masks, %obj);
                        %hitloc = getWords(%ray, 1, 3);
                    
                        if (isObject(getWord(%ray, 0))) {
                            initContainerBoxSearch(%hitloc, "0.5 0.5 0.5", $TypeMasks::StaticObjectType | $TypeMasks::ItemObjectType);
                            %next = containerSearchNext();
                            if(%next.owningSeat == %seat && %next.owningSeat !$= "")
                            {
                                %ev.PeakCards(%seat);
                                serverPlay3D(("cardPick" @ getRandom(1, 4) @ "Sound"), %next.getPosition());

                                return;
                            } 
                        }
                    }
                    else if (%player.peaking)
                    {

                        %ev.UnPeakCards(%seat);
                        serverPlay3D(("cardPick" @ getRandom(1, 4) @ "Sound"), %next.getPosition());

                        return;
                    }
                }
            }
        }
        parent::onTrigger(%this, %obj, %trig, %val);
    }

};
deactivatePackage("EventGame_texasHoldem");
activatePackage("EventGame_texasHoldem");