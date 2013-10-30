
module.exports = function(grunt) {
  grunt.initConfig({
    pkg: grunt.file.readJSON('package.json'),
    watch: {
      scripts: {
        files: ['game/**/*.js'],
        tasks: ['default'],
        options: {
          interrupt: true
        }
      }
    },
    copy: {
      build: {
        files: [
        ]
      }
    },
    browserify: {
      build: {
        src: ['game/game.js'],
        dest: 'dist/game.js'
      }
    },
    uglify: {
      build: {
        src: 'dist/game.js',
        dest: 'dist/game.min.js'
      }
    },
    clean: ['dist']
  });

  grunt.loadNpmTasks('grunt-contrib-watch');
  grunt.loadNpmTasks('grunt-contrib-copy');
  grunt.loadNpmTasks('grunt-contrib-uglify');
  grunt.loadNpmTasks('grunt-contrib-clean');
  grunt.loadNpmTasks('grunt-browserify');

  grunt.registerTask('default', ['copy', 'browserify', 'uglify']);
};
