# Author:: Nathaniel Talbott.
# Copyright:: Copyright (c) 2000-2002 Nathaniel Talbott. All rights reserved.
# License:: Ruby license.

require 'test/unit'
require 'adder'

class TC_Adder < Test::Unit::TestCase
  def setup
    @adder = Adder.new(5)
  end
  def test_add__2
    assert_equal(7, @adder.add(2), "Should have added correctly")
  end
    def test_addfail__3
    assert_equal(7, @adder.add(3), "Test failure")
  end
  def teardown
    @adder = nil
  end
end
