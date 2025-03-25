<Toriel><markPoint><stop...>Oh?<stop><stop> Hello, <stop>child.<stop><stop>
<markEnter>You look quite <blankEnter>confused<stop...>

<waitForUpdate><Toriel>
<markPoint>You don’t know why <blankEnter>I’m standing here?<stop><stop><stop><stop>
<markEnter>Because<stop...>
<waitForUpdate><Toriel><markPoint>I was placed here to 
<blankEnter>test the Ink language.

<waitForUpdate><Toriel><markPoint>However<stop...><blankEnter>since you’re here,
<waitForUpdate><Toriel><markPoint>Why don’t we make this <blankEnter>test <Toriel, Happy>a little <blankEnter>more interesting?<stop>

<waitForUpdate><Toriel><markPoint>Tell me,<stop><blankEnter>what brings you here?

 * [How does this work?] -> Clever
 * [I’m just looking around.] -> Casual
 * [...Can I not answer?] -> Shy

=== Clever ===
<Toriel, Happy><markPoint>Oh oh! <stop><Toriel, Side>You are full of 
<blankEnter>curiosity about this <blankEnter>world, <stop>aren’t you?<stop><stop>
<waitForUpdate><markPoint><Toriel, Happy>Very well, very well.
<waitForUpdate><Toriel><markPoint>However<stop...><blankEnter>even if you’re clever,<stop>
<waitForUpdate><Toriel><markPoint>You may not be able <blankEnter>to guess what <blankEnter>happens next.  
    -> Choice

=== Casual ===
<Toriel><markPoint>Hmm<stop...> <stop>It’s fine to <blankEnter>just look around.
<waitForUpdate><Toriel><markPoint>Sometimes, <blankEnter>taking it easy<blankEnter>is a good choice too.  
    -> Choice

=== Shy ===
<Toriel, Side><markPoint><stop...>Oh, child.<stop><markEnter>I’m not forcing you <blankEnter>to answer<stop...>
<waitForUpdate><markPoint><Toriel>If you don’t want <blankEnter>to speak, <stop>then don’t <blankEnter>force yourself.  
    -> Choice

=== Choice ===
<waitForUpdate><Toriel><markPoint>Then<stop...> let’s continue.

 * [What happens next?] -> Continue
 * [When will this test end?] -> Question
 * [...Can I leave?] -> Leave

=== Continue ===
<Toriel><markPoint>Alright then<stop...><waitForUpdate>
    -> MoreChoices

=== Question ===
<Toriel, Side><markPoint>Oh<stop...>are you already <blankEnter>eager to finish?<stop>
<waitForUpdate><Toriel><markPoint>Don’t worry, the end <blankEnter>will come eventually.<stop>
<waitForUpdate><Toriel, Side><markPoint>But at least, <blankEnter>stay and chat with me <blankEnter>for a while?  <waitForUpdate>
    -> MoreChoices

=== Leave ===
<Toriel, Side><markPoint>Oh, <stop>child<stop...>
<markEnter>Are you sure you want <blankEnter>to leave?
<waitForUpdate><markPoint><Toriel>This place <blankEnter>won’t trap you<stop...>
<waitForUpdate><markPoint><stop...>But once you leave, <blankEnter>
<Toriel, Side>you won’t be able to<blankEnter>see what happens next.

 * [...I think I’ll stay.] -> MoreChoices
 * [No, I’m leaving.] -> EndEarly

=== MoreChoices ===
<Toriel><markPoint>What do you <blankEnter>want to do next?

 * [Tell me a story.] -> Story
 * [I like choices.] -> MoreOptions
 * [...what you can say?] -> Meta

=== Story ===
<Toriel><markPoint>A story?<stop><markEnter>Alright, let me think<stop...>
<waitForUpdate><markPoint>Once upon a time<stop...>
<Toriel, Side><waitForUpdate><markPoint>Hmm<stop...>uh<stop...>ah<stop...><stop><stop><stop>
<markEnter>Or perhaps<stop...> we should <blankEnter>try something else?<waitForUpdate>
    -> MoreOptions
    
=== MoreOptions ===
<Toriel><markPoint><stop...>Here, <blankEnter>have a few more choices!

 * [Is the world a loop?] -> Loop
 * [About the template?] -> About
 * [I still want to hear a story.] -> Story
 * [I’m done choosing.] -> EndEarly

=== Loop ===
<Toriel, Side><markPoint>A loop, huh?
<waitForUpdate><markPoint><Toriel>If the world is a loop,
<blankEnter>then everything returns <blankEnter>to the beginning.
<waitForUpdate><Toriel, Side><markPoint>We would start over,<stop>
<blankEnter>make the same choices,<stop>
<blankEnter>say the same words.<stop...>
<waitForUpdate><Toriel><markPoint>But does that really <blankEnter>mean nothing changes?<stop>
<Toriel, Happy><markEnter>Hehe<stop...>who knows?<waitForUpdate> 
    -> MoreOptions

=== About ===
<Toriel, Side><markPoint>The template?<stop>
<markEnter>I don’t really <blankEnter>know much either.<stop...>
<waitForUpdate><Toriel><markPoint>After all, <blankEnter>you can't see the forest <blankEnter>for the trees.<waitForUpdate>
    -> MoreOptions

=== Meta ===
<Toriel, Happy><markPoint>Haha<stop...> you’re quite <blankEnter>straightforward.<stop>
<waitForUpdate><Toriel, Happy><markPoint>Alright, <stop>how much I <blankEnter>can say,<stop> depends on <blankEnter>how much you can do.<waitForUpdate>
    -> MoreOptions

=== EndEarly ===
<Toriel><markPoint>Then<stop...>this is it.<stop>
<markEnter>May your journey ahead,<stop><blankEnter>be smooth and safe.
<waitForUpdate><markPoint>Farewell, <stop>my child.  
    -> END
