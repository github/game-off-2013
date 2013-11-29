module.exports = function(grunt) {
    'use strict';

    grunt.initConfig({
        jshint: {
            all: [
                'package.json',
                'Gruntfile.js',
                'src/scripts/**/*.js',
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