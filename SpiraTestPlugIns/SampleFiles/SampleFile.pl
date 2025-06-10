#!/usr/bin/perl -w

# Specify our plan, how many tests we're writing
 use Test::More tests => 9;

 # or alternately, if we don't know how many:
 # use Test::More qw(no_plan);

 # Check that our module compiles and can be "use"d.
 BEGIN { use_ok( 'Inflectra::SpiraTest::Addons::Samples::TestMe' ); }

 # Check our module can be required. Very similar test to that above.
 require_ok( 'Inflectra::SpiraTest::Addons::Samples::TestMe' );

 # There are a number of ways to generate the "ok" tests. These are:
 # ok: first argument is true, second argument is name of test.
 # is: first argument equals (eq) second argument, third argument is name of test.
 # isnt: first argument does not equal (ne) the second, third is name of test
 # like: first argument matches regexp in second, third is name of test
 # unlike: first argument does not match regexp, third is name of test
 # cmp_ok: compares first and third argument with comparison in second. Forth is test name.

 # Here are some examples that should PASS
 ok( add(1,1) == 2, "Basic addition is working");

 is ( subtract(2,1), 1, "Basic subtraction is working");
 isnt( multiply(2,2), 5, "Basic multiplication doesn't fail");

# Here are some examples that should FAIL
 ok( add(1,1) == 3, "Basic addition is working");

 is ( subtract(2,1), 0, "Basic subtraction is working");
 isnt( multiply(2,2), 4, "Basic multiplication doesn't fail");

# Here is an example of a test that throws an ERROR
is($notdeclared, 1, "Undeclared variable test");
