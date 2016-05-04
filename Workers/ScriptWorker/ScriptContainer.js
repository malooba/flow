var branch, setOutflow;

result = (function (inputjson, script) {
    // Local scope is private
    var outflow = null;
    var outflows;
    // script sets result variables as properties of this object
    var result = {};

    // 'this' in the script scope 
    var scriptthis = {};

    // Global scope is available to scripts
    setOutflow = function (next) {
        if(outflows.indexOf(next) === -1)
            throw new Error('Invalid outflow');
        outflow = next;
    }

    branch = function (test) {
        setOutflow(test ? 'IfTrue' : 'IfFalse');
    };

    try {
        // split inputjson int arguments and values
        var inputs = JSON.parse(inputjson);
        outflows = inputs.$outflows;
        var args = [];
        var values = [];
        for(var input in inputs) {
            if (inputs.hasOwnProperty(input)) {
                args.push(input);
                values.push(inputs[input]);
            }
        }

        // add local result variable as an argument named result
        args.push('result');
        values.push(result);

        // add the script as the first argument
        args.push(script);

        // create a function from the script with the inputs (and result) as formal arguments
        var func = Function.apply({}, args);

        // Apply the function with scriptthis as this and the input values as argument values
        func.apply(scriptthis, values);

        // If outflow has been set then return it
        if(outflow) result.$outflow = outflow;

        // return final result
        return result;
    } catch(e) {
        Console.WriteLine(e.message);
        return JSON.stringify({ error: { message: e.message, stack: e.stack } });
    }
})('_inputjson_', '_script_');
