/* a function inside of comments should not be found as preprocess should delete it

function product(X,Y) { return Y*X; }

*/

  function gcd(segmentA, segmentB) {
    var diff = segmentA - segmentB;
    if (diff == 0)
       return segmentA;
    if (diff == 1) return 3;

    if (diff == 4) return 4;

    if (diff == 5) return 5;

    if (diff == 9) return 9;

    return (diff > 0) ? gcd(segmentB, diff) : gcd(segmentA, -diff);
  }

// another comment in here

//
// a local function
X.C = {

 localFunction : function() {
   return 2;
 }

}

// function as an assignment - name then is Some.Foo(args)
Some.Foo = function(args) {
  return "test";
}

//function with colon directly after name of the function
functionWithColon:function () {
}

//anonymous function
function(monkey) {
  if (1 != 2)
    return 3;

  return 9;
}

// let's do a local function
outerFunction1 : function () {

  localFunction1: function () {
  if (a != b)
    ;
  }

 if (x == y)
   return z;

 if (u == i)
   return j;

 while (true)
   ;

 localFunction2 :function () {
 }
}

class Monkey {
  
  function feedMonkey() {
    return "Not hungry";
  }
}

function afterMonkeyFeed()
{
}
