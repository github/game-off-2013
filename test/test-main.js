var tests = [];
for (var file in window.__karma__.files) {
    if (/test\.js$/.test(file)) {
        tests.push(file);
    }
}

requirejs.config({
    // Karma serves files from '/base'
    baseUrl: '/base/src/scripts',

    paths: {
        'd3': '../lib/d3.v3.min',
        'geodesic': '../lib/geodesic'
    },

    shim: {
        'd3': {
            exports: 'd3'
        },
        'geodesic': ['d3']
    },

    // ask Require.js to load these files (all our tests)
    deps: tests,

    // start test run, once Require.js is done
    callback: window.__karma__.start
});