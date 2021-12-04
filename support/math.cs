function signedRemainder(%a, %b)
{
    return %a - %b*atoi(%a/%b);
}

function mod(%a, %b)
{
    return signedRemainder(signedRemainder(%a, %b) + %b, %b);
}