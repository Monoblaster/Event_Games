function signedRemainder(%a, %b)
{
    return %a - %b*atoi(%a/%b);
}

function mod(%a, %b)
{
    return signedRemainder(signedRemainder(%a, %b) + %b, %b);
}

function vectorRotate(%vec, %axis, %angle)
{
    if (vectorLen(%axis) != 1)
    {
        %axis = vectorNormalize(%axis);
    }

    %proj = vectorScale(%axis, vectorDot(%vec, %axis));
    %ortho = vectorSub(%vec, %proj);
    %w = vectorCross(%axis, %ortho);
    %cos = mCos(%angle);
    %sin = mSin(%angle);
    %x1 = %cos / vectorLen(%ortho);
    %x2 = %sin / vectorLen(%w);
    %rotOrtho = vectorScale(vectorAdd(vectorScale(%ortho, %x1), vectorScale(%w, %x2)), vectorLen(%ortho));
    return vectorAdd(%rotOrtho, %proj);
}

function vectorToRotUp(%vector)  
{  
    %vector = vectorNormalize(%vector);

    %xyz = vectorNormalize(vectorCross("1 0 0", %vector)); //rotation axis
    %u = mACos(vectorDot("1 0 0", %vector)) * -1; //rotation value
    return %xyz SPC %u;
}