# Scheduler
---------
*CoreTaskScheduler* is an abstract class that enables scheduling task based on priority.
I decided to implement two separate implementations of the scheduler, rather than using
a flag indicating the purpose of that scheduler, because it was faster for me (easier to read code).
To use this scheduler, the user is not creating ordinary Task object, but a specific one
*PriorityLimitedTask*, that has maximum duration as a limit, has a priority and enables cooperation (*InvalidTaskException* can be thrown if scheduler is called with wrong type of a task).
As I was unable to find thread-safe ordered collection, I am sorting it manually, using *SortPendingTasks()* method, where sorting is done in parallel.

### Registering task on a scheduler
For a task to become part of my scheduler, it needs to go through QueueTask(Task) method. Each time a task has been scheduled, RunScheduling() is called. To be able to cooperatively close running tasks, I use default .NET scheduler. Each time a user's task is run on my scheduler, I start a new *callback* task using .NET default scheduler. This callback's purpose is to free thread for a specified amount of time  (time needed for the corresponding user's task to finish) and then cancelling the user's task using a cooperative mechanism.

### Cooperative mechanism
Cooperative cancelling and context-switch (Preemptive scheduling) depends on how a user will implement an action sent to my scheduler. Each action is meant to provide an execution point when it can be cancelled or paused. If that point does not exist, a cooperative mechanism is not used.
Pausing is explained in Preemptive Scheduler. A user can specify more than one execution point when their action can be paused or cancelled. My scheduler has a list of tasks running in the background, that is responsible for scheduler's side of cooperation.

> As a deadlock prevention mechanism, I used Banker's algorithm.

## Non-Preemptive scheduler
Task runs until it reaches its duration limit. After that limit is reached, using a cooperative mechanism, a task is stopped from further execution.
Number of user's task running in parallel must not exceed specified maximum level of parallelism.

## Preemptive scheduler
