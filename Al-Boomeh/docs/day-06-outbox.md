### Incident log :
i add the line `Environment.Exit(1)` after the `await transaction.CommitAsync();` and the order was saved in the database with Id `300046` but in the log
no notifications was found its mean because the test line "sever crached" between to oprations

## the delay in orderplace :
03:11:07.9744714 − 03:11:06.8974400 = 1.0770314 s ≈ 1077 ms
this the time between order saved in the database and snet the notification 
Background Service was used with polling 500ms
