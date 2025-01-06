using System;
using System.Collections.Generic;

public class Queue
{
    //public int id { get; set; }
    public int? queue_no { get; set; }
    public int? customer_amount { get; set; }
    public int? run_queue_type_id { get; set; }
    public int? wait_queue_count { get; set; }
    public DateTime? call_at { get; set; }
    public bool? is_latest { get; set; }
    public bool?  read_only { get; set; }
    public List<string> editable_fields { get; set; } = new List<string>();
    public string language { get; set; } = string.Empty;
}

public class QueueModel
{
    public Shop shop { get; set; }
    public Queue queue { get; set; }
    public string language { get; set; } = string.Empty;
}
