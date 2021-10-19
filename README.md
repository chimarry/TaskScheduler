# Task scheduler

[![Build Status](https://travis-ci.org/joemccann/dillinger.svg?branch=master)](https://travis-ci.org/joemccann/dillinger)

The purpose of this project is to implement a task scheduler that is a subclass of  System.Threading.Tasks.TaskScheduler class. The number of threads that the task scheduler uses can be changed. Preemptive (PIP or PCP) and non-preemptive scheduling must be enabled. 
Using [Banker's algorithm](https://en.wikipedia.org/wiki/Banker%27s_algorithm) prevent deadlock situations (a mechanism that enables controlled access to shared resources).  
Each task that needs to be executed has a certain duration. Enable task scheduling in real-time (adding a new task in real-time).

## Building and running application
In order to run the application, .NET Core 3.1 must be installed.

## Details of implementation
---------
*CoreTaskScheduler* is an abstract class that enables scheduling tasks based on priority.  
I decided to implement two separate implementations of the scheduler, rather than using 
a flag indicating the purpose of that scheduler, because it was faster for me (easier to read code). 
To use this scheduler, the user is not creating an ordinary Task object, but a specific one  
*PriorityLimitedTask*, which has maximum duration as a limit, has a priority and enables cooperation (*InvalidTaskException* can be thrown if the scheduler is called with the wrong type of a task).
As I was unable to find thread-safe ordered collection, I am sorting it manually, using *SortPendingTasks()* method, where sorting is done in parallel.

### Registering task on a scheduler
For a task to become part of my scheduler, it needs to go through QueueTask(Task) method. Each time a task has been scheduled, RunScheduling() is called. To be able to cooperatively close running tasks, I use the default .NET scheduler. Each time a user's task is run on my scheduler, I start a new *callback* task using .NET default scheduler. This callback's purpose is to free the thread for a specified amount of time  (time needed for the corresponding user's task to finish) and then cancel the user's task using a cooperative mechanism. 

### Cooperative mechanism
Cooperative cancelling and context-switch (Preemptive scheduling) depend on how a user will implement an action sent to my scheduler. Each action is meant to provide an execution point when it can be cancelled or paused. If that point does not exist, a cooperative mechanism is not used. 
Pausing is explained in Preemptive Scheduler. A user can specify more than one execution point when their action can be paused or cancelled. My scheduler has a list of tasks running in the background, that is responsible for the scheduler's side of cooperation.

> As a deadlock prevention mechanism, I used Banker's algorithm.

## Non-Preemptive scheduler
Task runs until it reaches its duration limit. After that limit is reached, using a cooperative mechanism, a task is stopped from further execution. 
A number of user's tasks running in parallel must not exceed a specified maximum level of parallelism.

## Preemptive scheduler
Task A has Priority.Highest. There are three currently executing the task, two of them with Priority.Normal, and one (Task D) with Priority.Lowest. Task A would stop the execution of a task with  
minimum priority, in this case, task D, but task D does not allow pausing. So algorithm chooses  
a task B, because it can be paused. Now, tasks A, C, D are executing, and task B is returned in  
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
    cooperativeMechanism.CanBePaused = true;
    if(coooperativeMechanism.IsPaused)
        Task.Delay(cooperativeMechanism.PausedFor).Wait();
    cooperativeMechanism.CanBePaused = false;
     // Doing something
    Thread.Sleep(2000);
}
~~~~~~~

As we can see, pausing (context-switching) is done with some limitations. There is no guaranty that the task that is paused will continue executing on the same thread, or after task A finishes. So, this context switching is done differently than CPU context switching, but the idea is the same.  A callback thread, that is run on the default scheduler, checks possible pausing as well as cancelling.

## Banker's algorithm
-------------------------
### Introduction
See [About algorithm](https://en.wikipedia.org/wiki/Banker%27s_algorithm)

### Usage
In CoreTaskScheduler class there is a method *GetNextTaskWithDeadlockAvoidance* that finds  
the first task in a queue of pending tasks that can allocate resources, if necessary.  
In PreemptiveTaskScheduler, if there is a need for context-switch, the algorithm is used directly  
to check whether interrupt should be acknowledged.  
Implementation depends on different dictionary structures, and to avoid a race condition,  
a C# locking mechanism is used.