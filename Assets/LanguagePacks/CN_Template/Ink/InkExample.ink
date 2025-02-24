<Toriel><markPoint><stop...>哦？<stop><stop>你好，<stop>孩子。<stop><stop>
<markEnter>你看起来很困惑呢<stop...>

<waitForUpdate>
<markPoint>不知道我为什么站在这么？<stop><stop><stop><stop>
<markEnter>因为<stop...>我被安排在此测试
<blankEnter>Ink语言能否在模板中使用。

<waitForUpdate><markPoint>不过<stop...>既然你来了，
<blankEnter><stop>我们不妨让这个测试，<blankEnter><Toriel, Happy>变得稍微有趣一点吧？<stop>

<waitForUpdate><markPoint>告诉我，你来这里想干什么？

 * [我想知道这儿是怎么运作的！] -> Clever
 * [我只是随便看看。] -> Casual
 * [...我可以不回答吗？] -> Shy

=== Clever ===
<Toriel, Happy><markPoint>哦哦！<stop><Toriel, Side>你对这个世界
<blankEnter>充满了好奇，<stop>对吧？<stop><stop>
<markEnter><Toriel, Happy>很好很好。
<waitForUpdate><Toriel><markPoint>不过<stop...>就算你聪明，<stop>
<blankEnter>也未必能猜到<blankEnter>接下来会发生什么。  
    -> Choice

=== Casual ===
<Toriel><markPoint>嗯<stop...><stop>随意看看也没关系。
<markEnter>有时候，放松一下<blankEnter>也是不错的选择。  
    -> Choice

=== Shy ===
<Toriel, Side><markPoint><stop...>哦，孩子。<stop><blankEnter>我不是要逼你回答什么<stop...>
<waitForUpdate><markPoint>如果你不想说，<blankEnter><stop>那就不用勉强自己了。  
    -> Choice

=== Choice ===
<waitForUpdate><Toriel><markPoint>那么<stop...>让我们继续吧。

 * [我想看看接下来会发生什么。] -> Continue
 * [这段测试要到什么时候才会结束？] -> Question
 * [...我可以离开吗？] -> Leave

=== Continue ===
<Toriel><markPoint>那么好<stop...><waitForUpdate>
    -> MoreChoices

=== Question ===
<Toriel, Side><markPoint>哦<stop...>已经等不及想结束了？<stop>
<Toriel><markEnter>放心，终点总会到来的。<stop>
<Toriel, Side><markEnter>但至少，先陪我聊一会儿吧？  <waitForUpdate>
    -> MoreChoices

=== Leave ===
<Toriel, Side><markPoint>哦，<stop>孩子<stop...>
<markEnter>你确定要离开吗？
<Toriel><markEnter>这里并不会困住你<stop...>
<waitForUpdate><markPoint><stop...>但一旦离开，
<Toriel, Side>你就无法<blankEnter>看到后面会发生什么了哦？

 * [...我还是留下吧。] -> MoreChoices
 * [不，我还是走了。] -> EndEarly

=== MoreChoices ===
<Toriel><markPoint>接下来，你想做什么呢？

 * [告诉我一个故事吧。] -> Story
 * [再给我一个选择，我喜欢选择。] -> MoreOptions
 * [...我只想知道你都能说什么。] -> Meta

=== Story ===
<Toriel><markPoint>故事吗<stop...>好吧，我想想<stop...>
<waitForUpdate><markPoint>很久很久以前——
<Toriel, Side><waitForUpdate><markPoint>嗯<stop...>额<stop...>啊<stop...><stop><stop><stop>
<markEnter>要不然<stop...>我们换个方式？  
    -> MoreOptions
    
=== MoreOptions ===
<waitForUpdate><Toriel><markPoint><stop...>再给你几个选项吧！

 * [世界是一个循环么？] -> Loop
 * [你能告诉我一些关于模板的事吗？] -> About
 * [我还是想听个故事。] -> Story
 * [我不想再选什么了。] -> EndEarly

=== Loop ===
<Toriel, Side><markPoint>循环啊<stop...><stop>
<Toriel><markEnter>如果世界是个循环，
<blankEnter>那么一切都会回到原点。
<waitForUpdate><Toriel, Side><markPoint>我们会重新开始，<stop>
<blankEnter>做出相同的选择，<stop>
<blankEnter>说出相同的话<stop...>
<waitForUpdate><Toriel><markPoint>但这真的意味着<blankEnter>什么都不会改变吗？<stop>
<Toriel, Happy><markEnter>呵呵<stop...>谁知道呢？  
    -> MoreOptions

=== About ===
<Toriel, Side><markPoint>模板吗？<stop>我也不太清楚<stop...>
<Toriel><markEnter>毕竟，<stop>不识庐山真面目，<blankEnter><stop>只缘身在此山中嘛。
    -> MoreOptions

=== Meta ===
<Toriel, Happy><markPoint>哈哈<stop...>你倒是很坦率呢。<stop>
<markEnter>好吧，<stop>我能说多少话，<stop>
<blankEnter>取决于你能做多少事。
    -> MoreOptions

=== EndEarly ===
<Toriel><markPoint>那么<stop...>就到这里吧。<stop>
<markEnter>愿你在接下来的旅程中，<stop><blankEnter>一切顺利。
<waitForUpdate><markPoint>再见了，<stop>孩子。  
    -> END
