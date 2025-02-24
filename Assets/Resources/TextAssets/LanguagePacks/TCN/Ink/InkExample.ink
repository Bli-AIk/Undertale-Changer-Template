<Toriel><markPoint><stop...>哦？<stop><stop>你好，<stop>孩子。<stop><stop>  
<markEnter>你看起來很困惑呢<stop...>  

<waitForUpdate>  
<markPoint>不知道我為什麼站在這裡？<stop><stop><stop><stop>  
<markEnter>因為<stop...>我被安排在此測試  
<blankEnter>Ink語言能否在模板中使用。  

<waitForUpdate><markPoint>不過<stop...>既然你來了，  
<blankEnter><stop>我們不妨讓這個測試，<blankEnter><Toriel, Happy>變得稍微有趣一點吧？<stop>  

<waitForUpdate><markPoint>告訴我，你來這裡想做什麼？  

 * [我想知道這裡是怎麼運作的！] -> Clever  
 * [我只是隨便看看。] -> Casual  
 * [...我可以不回答嗎？] -> Shy  

=== Clever ===  
<Toriel, Happy><markPoint>哦哦！<stop><Toriel, Side>你對這個世界  
<blankEnter>充滿了好奇，<stop>對吧？<stop><stop>  
<markEnter><Toriel, Happy>很好很好。  
<waitForUpdate><Toriel><markPoint>不過<stop...>就算你聰明，<stop>  
<blankEnter>也未必能猜到<blankEnter>接下來會發生什麼。  
    -> Choice  

=== Casual ===  
<Toriel><markPoint>嗯<stop...><stop>隨意看看也沒關係。  
<markEnter>有時候，放鬆一下<blankEnter>也是不錯的選擇。  
    -> Choice  

=== Shy ===  
<Toriel, Side><markPoint><stop...>哦，孩子。<stop><blankEnter>我不是要逼你回答什麼<stop...>  
<waitForUpdate><markPoint>如果你不想說，<blankEnter><stop>那就不用勉強自己了。  
    -> Choice  

=== Choice ===  
<waitForUpdate><Toriel><markPoint>那麼<stop...>讓我們繼續吧。  

 * [我想看看接下來會發生什麼。] -> Continue  
 * [這段測試要到什麼時候才會結束？] -> Question  
 * [...我可以離開嗎？] -> Leave  

=== Continue ===  
<Toriel><markPoint>那麼好<stop...><waitForUpdate>  
    -> MoreChoices  

=== Question ===  
<Toriel, Side><markPoint>哦<stop...>已經等不及想結束了？<stop>  
<Toriel><markEnter>放心，終點總會到來的。<stop>  
<Toriel, Side><markEnter>但至少，先陪我聊一會兒吧？  <waitForUpdate>  
    -> MoreChoices  

=== Leave ===  
<Toriel, Side><markPoint>哦，<stop>孩子<stop...>  
<markEnter>你確定要離開嗎？  
<Toriel><markEnter>這裡並不會困住你<stop...>  
<waitForUpdate><markPoint><stop...>但一旦離開，  
<Toriel, Side>你就無法<blankEnter>看到後面會發生什麼了哦？  

 * [...我還是留下吧。] -> MoreChoices  
 * [不，我還是走了。] -> EndEarly  

=== MoreChoices ===  
<Toriel><markPoint>接下來，你想做什麼呢？  

 * [告訴我一個故事吧。] -> Story  
 * [再給我一個選擇，我喜歡選擇。] -> MoreOptions  
 * [...我只想知道你都能說什麼。] -> Meta  

=== Story ===  
<Toriel><markPoint>故事嗎<stop...>好吧，我想想<stop...>  
<waitForUpdate><markPoint>很久很久以前——  
<Toriel, Side><waitForUpdate><markPoint>嗯<stop...>呃<stop...>啊<stop...><stop><stop><stop>  
<markEnter>要不然<stop...>我們換個方式？  
    -> MoreOptions  

=== MoreOptions ===  
<waitForUpdate><Toriel><markPoint><stop...>再給你幾個選項吧！  

 * [世界是一個循環嗎？] -> Loop  
 * [你能告訴我一些關於模板的事嗎？] -> About  
 * [我還是想聽個故事。] -> Story  
 * [我不想再選什麼了。] -> EndEarly  

=== Loop ===  
<Toriel, Side><markPoint>循環啊<stop...><stop>  
<Toriel><markEnter>如果世界是一個循環，  
<blankEnter>那麼一切都會回到原點。  
<waitForUpdate><Toriel, Side><markPoint>我們會重新開始，<stop>  
<blankEnter>做出相同的選擇，<stop>  
<blankEnter>說出相同的話<stop...>  
<waitForUpdate><Toriel><markPoint>但這真的意味著<blankEnter>什麼都不會改變嗎？<stop>  
<Toriel, Happy><markEnter>呵呵<stop...>誰知道呢？  
    -> MoreOptions  

=== About ===  
<Toriel, Side><markPoint>模板嗎？<stop>我也不太清楚<stop...>  
<Toriel><markEnter>畢竟，<stop>不識廬山真面目，<blankEnter><stop>只緣身在此山中嘛。  
    -> MoreOptions  

=== Meta ===  
<Toriel, Happy><markPoint>哈哈<stop...>你倒是很坦率呢。<stop>  
<markEnter>好吧，<stop>我能說多少話，<stop>  
<blankEnter>取決於你能做多少事。  
    -> MoreOptions  

=== EndEarly ===  
<Toriel><markPoint>那麼<stop...>就到這裡吧。<stop>  
<markEnter>願你在接下來的旅程中，<stop><blankEnter>一切順利。  
<waitForUpdate><markPoint>再見了，<stop>孩子。  
    -> END  
