"""Loopback-only stand-in for OW3N's hard-coded roll service and Discord webhook."""

import json
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import unquote, urlparse


class Handler(BaseHTTPRequestHandler):
    def do_GET(self):
        if urlparse(self.path).path == "/healthz":
            self.send_response(200)
            self.end_headers()
            self.wfile.write(b"ok\n")
            return
        self.send_error(404)

    def do_POST(self):
        path = urlparse(self.path).path
        if path == "/webhook":
            self.send_response(204)
            self.end_headers()
            return

        parts = path.split("/", 3)
        if len(parts) == 4 and parts[1] == "roll":
            formula = unquote(parts[3])
            payload = {
                "rolls": [{"result": 1, "expression": formula}],
                "result": 1,
                "message": f"stub roll: {formula}",
            }
            body = json.dumps(payload).encode()
            self.send_response(200)
            self.send_header("Content-Type", "application/json")
            self.send_header("Content-Length", str(len(body)))
            self.end_headers()
            self.wfile.write(body)
            return

        self.send_error(404)

    def log_message(self, _format, *_args):
        return


ThreadingHTTPServer(("127.0.0.1", 3000), Handler).serve_forever()
