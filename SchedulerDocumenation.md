# Scheduler
---------
*CoreTaskScheduler* is an abstract class that enables scheduling task based on priority.  
I decided to implement two separate implementations of the scheduler, rather than using 
a flag indicating the purpose of that scheduler, because it was faster for me (easier to read code). 
To use this scheduler, the user is not creating ordinary Task object, but a specific one  
*PriorityLimitedTask*, that has maximum duration as a limit, has a priority and enables cooperation (*InvalidTaskException* can be thrown if the scheduler is called with the wrong type of a task).
As I was unable to find thread-safe ordered collection, I am sorting it manually, using *SortPendingTasks()* method, where sorting is done in parallel.

### Registering task on a scheduler
For a task to become part of my scheduler, it needs to go through QueueTask(Task) method. Each time a task has been scheduled, RunScheduling() is called. To be able to cooperatively close running tasks, I use default .NET scheduler. Each time a user's task is run on my scheduler, I start a new *callback* task using .NET default scheduler. This callback's purpose is to free thread for a specified amount of time  (time needed for the corresponding user's task to finish) and then cancelling the user's task using a cooperative mechanism. 

### Cooperative mechanism
Cooperative cancelling and context-switch (Preemptive scheduling) depends on how a user will implement an action sent to my scheduler. Each action is meant to provide an execution point when it can be cancelled or paused. If that point does not exist, a cooperative mechanism is not used. 
Pausing is explained in Preemptive Scheduler. A user can specify more than one execution point when their action can be paused or cancelled. My scheduler has a list of tasks running in the background, that is responsible for scheduler's side of cooperation.

> As a deadlock prevention mechanism, I used Banker's algorithm.

## Non-Preemptive scheduler
Task runs until it reaches its duration limit. After that limit is reached, using a cooperative mechanism, a task is stopped from further execution. 
A number of user's task running in parallel must not exceed a specified maximum level of parallelism.

## Preemptive scheduler
Task A has Priority.Highest. There are three currently executing task, two of them with Priority.Normal, and one (Task D) with Priority.Lowest. Task A would stop the execution of a task with  
minimum priority, in this case, task D, but task D does not allow pausing. So algorithm chooses  
a task B, because it can be paused. Now, task A, C, D are executing, and task B is returned in  
queue with pending tasks. Pausing is done using a cooperative mechanism. As I mentioned before,  
a user function must define execution points where it can be paused. For example:
~~~~~~
Action action = () => 
{
    while(true){
        Console.WriteLine("I can be paused anytime.");
        if(coooperativeMechanism.IsPaused)
            Task.Delay(cooperativeMechanism.PausedFor).Wait();
    }
}
~~~~~~~

~~~~~~
Action actionWithSpecificPauseTime = () => 
{
    Thread.Sleep(4000);
    Console.WriteLine("I can be only if my IsPaused is set to true before 4000ms expires.");
    if(coooperativeMechanism.IsPaused)
        Task.Delay(cooperativeMechanism.PausedFor).Wait();
     // Doing something
    Thread.Sleep(2000);
}
~~~~~~~

As we can see, pausing (context-switching) is done with some limitations. There is no guaranty that task that is paused will continue executing on the same thread, or after the task A finishes. So, this context switching is done differently than CPU context switching, but the idea is the same.  A callback thread, that is run on default scheduler, checks possible pausing as well as cancelling.

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
