using UnityEngine;
using System.Collections;
using System.Threading;

namespace Pathfinding.Threading {
	public class Parallel {
		
		Parallel Instance;
		
		AutoResetEvent[] jobAvailable;
		ManualResetEvent[] threadIdle;
		Thread[] threads;
		
		static System.Object sync = new System.Object ();
		
		int currentIndex;
		int stopIndex;
		
		int step = 1;
		
		public delegate void ForLoopBody (int i);
		
		ForLoopBody loopBody;
		
		public static int threadsCount = System.Environment.ProcessorCount;
		public static int iterationStepLength = 10;
		
		// Initialize Parallel class's instance creating required number of threads
		// and synchronization objects
		private void Initialize( )
		{
			threadsCount = System.Environment.ProcessorCount;
			
			//No point starting new threads for a single core computer
			if (threadsCount <= 1) {
				return;
			}
			
			// array of events, which signal about available job
			jobAvailable = new AutoResetEvent[threadsCount];
			// array of events, which signal about available thread
			threadIdle = new ManualResetEvent[threadsCount];
			// array of threads
			threads = new Thread[threadsCount];
		
			for ( int i = 0; i < threadsCount; i++ )
			{
				jobAvailable[i] = new AutoResetEvent( false );
				threadIdle[i]   = new ManualResetEvent( true );
		
				threads[i] = new Thread( new ParameterizedThreadStart( WorkerThread ) );
				threads[i].IsBackground = false;
				threads[i].Start( i );
			}
		}
		
		public void Close () {
			//Exit all threads
			loopBody = null;
			for ( int i = 0; i < threadsCount; i++ )
			{
				jobAvailable[i].Set( );
			}
		}
		
		public static void For( int start, int stop, ForLoopBody loopBody  )
		{
			For (start,stop,1,loopBody);
		}
		
		public static void For( int start, int stop, int stepLength, ForLoopBody loopBody  )
		{
			For (start,stop,stepLength,loopBody,true);
		}
		
		public static void For( int start, int stop, int stepLength, ForLoopBody loopBody, bool close  )
		{
			// get instance of parallel computation manager
			Parallel instance = new Parallel ();
			instance.Initialize ();
			instance.ForLoop (start,stop,stepLength,loopBody, close);
		}
		
			
		public void ForLoop ( int start, int stop, int stepLength, ForLoopBody loopBody, bool close  )
		{
			lock ( sync )
			{
				
				//Parallel instance = new Parallel ();
				
				
				stepLength = stepLength < 1 ? 1 : stepLength;
				
				currentIndex	= start;
				stopIndex		= stop;
				this.loopBody	= loopBody;
				step			= stepLength;
				//No point starting new threads for a single core computer, just run it on this thread
				if (threadsCount <= 1) {
					for (int i=start;i<stop;i+= stepLength) {
						loopBody( i );
					}
					return;
				}
				
				// signal about available job for all threads and mark them busy
				for ( int i = 0; i < threadsCount; i++ )
				{
					threadIdle[i].Reset( );
					jobAvailable[i].Set( );
				}
				
				// wait until all threads become idle
				for ( int i = 0; i < threadsCount; i++ )
				{
					threadIdle[i].WaitOne( );
				}
				
				if (close) {
					Close ();
				}
			}
		}
		
		// Worker thread performing parallel computations in loop
		private void WorkerThread( object index )
		{
			int threadIndex = (int) index;
			int localIndex = 0;
		
			while ( true )
			{
				// wait until there is job to do
				jobAvailable[threadIndex].WaitOne( );
		
				// exit on null body
				if ( loopBody == null ) {
					break;
		
				}
				
				while ( true )
				{
					// get local index incrementing global loop's current index
					//localIndex = Interlocked.Increment( ref currentIndex );
					
					localIndex = Interlocked.Add (ref currentIndex, iterationStepLength*step);
					
					int startIndex = localIndex-iterationStepLength*step;
					
					bool doBreak = false;
					
					if (localIndex >= stopIndex) {
						doBreak = true;
						localIndex = stopIndex;
					}
					
					for (int i=startIndex;i<localIndex;i+= step) {
						
						loopBody( i );
					}
					
					if (doBreak) {
						break;
					}
					// run loop's body
					//loopBody( localIndex );
				}
		
				// signal about thread availability
				threadIdle[threadIndex].Set( );
			}
		}
	}
}