using System.Collections;
using System.Collections.Generic;

public class pointQueue {

	public Queue queue = new Queue();
	
	public void addtoQueue(LidarPoint lidarpoint){
		queue.Enqueue (lidarpoint);
	}
}
