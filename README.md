A simple library for creating single instance applications.

```c#
//Set this to be some unique string, maybe typeof(Program).AssemblyQualifiedName.
var id = "<your application identifier>";
using (var server = new Server(id))
{
  //Check IsDisposed to determine whether we're the first instance or not.
  //If IsDisposed is false, we're the main instance.
  if (server.IsDisposed)
  {
    //We're a secondary instance.
    var client = new Client(id);
    //Send a message to the main instance.
    client.Send(args);
    //Exit secondary instance.
    return;
  }
  //We're the main instance, listen for messages.
  server.Message += (sender, e) =>
  {
    //Do something with e.Message.
  };
}
```
