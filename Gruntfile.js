module.exports = function(grunt) {
    'use strict';

    grunt.initConfig({
        jshint: {
            all: [
                'Gruntfile.js',
                'src/**/*.js',
                'spec/**/*.js'
            ],
            options: {
                jshintrc: '.jshintrc'
            }
        }
    });

    grunt.loadNpmTasks('grunt-contrib-jshint');

    grunt.registerTask('test', ['jshint']);

    grunt.registerTask('default', ['test']);

};