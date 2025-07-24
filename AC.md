# match result AC

## score -> display result
### Matchid=1, "0:0 (First Half)"
Given match id has no recoed in repo, then return "0:0 (First Half)"

### Matchid=1, 1:0 (First Half)
- given Matchid=1 And MatchResult = ""
- when Update Matchid=1, MatchEvent=HomeGoal
- then matchresult should be "H" And display result should see "1:0 (First Half)"

### Given Match id 1 and score is 1:0 (First Half) - AwayGoal
- Given MatchId = 1 And MatchResult = "H"
- When update MatchId = 1 And MatchEvent = AwayGoal
- Then MatchResult should be "HA" And Display result should be "1:1 (First Half)"

### Given Match id 1 and score is 1:1 (First Half) - NextPeriod
- Given MatchId = 1 And MatchResult = "HA"
- When update MatchId = 1 And MatchEvent = NextPeriod
- Then MatchResult should be "HA;" And Display result should be "1:1 (Second Half)"

### Given Match id 1 and score is 1:1 (Second Half) - AwayCancel
- Given MatchId = 1 and MatchResult = "HA;"
- When update MatchId = 1 And MatchEvent = AwayCancel
- Then MatchResult should be "H;" And Display result should be "1:0 (Second Half)"

### Given Match id 1 and score is 1:1 (First Half) - HomeCancel
- Given MatchId = 1 And MatchResult = "HA"
- When update MatchId = 1 And MatchEvent = HomeCancel
- Then MatchResult should be "A" And Display result should be "0:1 (First Half)"

### Given Match id 1 and score is 2:1 (First Half) - HomeCancel (multiple goals)
- Given MatchId = 1 And MatchResult = "HAH"
- When update MatchId = 1 And MatchEvent = HomeCancel
- Then MatchResult should be "HA" And Display result should be "1:1 (First Half)"

### Given Match id 1 and score is 1:2 (Second Half) - AwayCancel (multiple goals)
- Given MatchId = 1 And MatchResult = "H;AA"
- When update MatchId = 1 And MatchEvent = AwayCancel
- Then MatchResult should be "H;A" And Display result should be "1:1 (Second Half)"

## Edge Cases

### NextPeriod called twice (should not add extra semicolon)
- Given MatchId = 1 And MatchResult = "H;"
- When update MatchId = 1 And MatchEvent = NextPeriod
- Then MatchResult should be "H;" And Display result should be "1:0 (Second Half)"

## Cancel Failure Cases (when last character doesn't match after removing semicolon)

### Cancel fail, cannot cancel Away when final character is not Away
- Given MatchId = 1 And MatchResult = "HHA;H"
- When Update sequence: AwayCancel  
- Then should Throw UpdateMatchResultException
- And Message is "1, AwayCancel + HHA;H"
- Because after removing ";" → "HHAH", last character is 'H' not 'A'

### Cancel fail, cannot cancel Home when final character is not Home
- Given MatchId = 1 And MatchResult = "AHA;A"
- When Update sequence: HomeCancel
- Then should Throw UpdateMatchResultException
- And Message is "1, HomeCancel + AHA;A" 
- Because after removing ";" → "AHAA", last character is 'A' not 'H'

### Cancel fail, cannot cancel when empty result
- Given MatchId = 1 And MatchResult = ""
- When Update sequence: HomeCancel or AwayCancel
- Then should Throw UpdateMatchResultException

## Complex Scenarios

### Multiple events in sequence
- Given MatchId = 1 And MatchResult = ""
- When update sequence: HomeGoal -> AwayGoal -> HomeGoal -> NextPeriod -> AwayGoal -> AwayCancel -> HomeGoal
- Then final MatchResult should be "HAH;H" And Display result should be "3:1 (Second Half)"

### Cancel and re-add goals
- Given MatchId = 1 And MatchResult = "HA"
- When update sequence: HomeCancel -> HomeGoal -> NextPeriod
- Then final MatchResult should be "AH;" And Display result should be "1:1 (Second Half)"