import random
import unittest
import spiratestextension

# sample PyUnit test case
class TestSequenceFunctions(unittest.TestCase):

	def setUp(self):
		self.seq = range(10)

	def testshuffle__2(self):
	# make sure the shuffled sequence does not lose any elements
		random.shuffle(self.seq)
		self.seq.sort()
		self.assertEqual(self.seq, range(10))

	def testchoice__3(self):
		element = random.choice(self.seq)
		self.assert_(element in self.seq)
		
	def testfail__4(self):
		self.assertEqual(1, 2, "1==2 Should fail")

	def testsample__5(self):
		self.assertRaises(ValueError, random.sample, self.seq, 20)
		for element in random.sample(self.seq, 5):
		    self.assert_(element in self.seq)

suite = unittest.TestLoader().loadTestsFromTestCase(TestSequenceFunctions)
testResult = unittest.TextTestRunner(verbosity=2).run(suite)
releaseId = 1
testSetId = 1
runnerName = 'PyUnit'
spiraTestExtension = spiratestextension.SpiraTestExtension()
spiraTestExtension.projectId = 1
spiraTestExtension.server = "localhost"
spiraTestExtension.port = 80
spiraTestExtension.path = "SpiraTest"
spiraTestExtension.userName = "fredbloggs"
spiraTestExtension.password = "fredbloggs"
spiraTestExtension.recordResults(TestSequenceFunctions, testResult, releaseId, testSetId, runnerName)
